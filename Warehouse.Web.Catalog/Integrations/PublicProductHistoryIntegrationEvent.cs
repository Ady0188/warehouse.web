using MediatR;
using System.Text.Json;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Catalog.Integrations;

internal class PublicProductHistoryIntegrationEvent : INotificationHandler<ProductHistoryEvent>
{
    private readonly IMediator _mediator;

    public PublicProductHistoryIntegrationEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(ProductHistoryEvent notification, CancellationToken cancellationToken)
    {
        var dto = new HistoryDto
        {
            StoreName = notification.UserStoreName,
            UserName = notification.UserName,
            Method = notification.Method,
            NewData = JsonSerializer.Serialize(notification.NewProduct),
            ObjectId = notification.NewProduct.Id,
            ObjectName = nameof(Product)
        };

        if (notification.OldProduct is not null)
            dto.OldData = JsonSerializer.Serialize(notification.OldProduct);

        var integrationEvent = new HistoryCreatedIntegrationEvent(dto);

        await _mediator.Publish(integrationEvent);
    }
}
