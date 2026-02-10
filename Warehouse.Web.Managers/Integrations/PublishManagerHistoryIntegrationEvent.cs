using MediatR;
using System.Text.Json;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Managers.Integrations;

internal class PublishManagerHistoryIntegrationEvent : INotificationHandler<ManagerHistoryEvent>
{
    private readonly IMediator _mediator;

    public PublishManagerHistoryIntegrationEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(ManagerHistoryEvent notification, CancellationToken cancellationToken)
    {
        var dto = new HistoryDto
        {
            StoreName = notification.StoreName,
            UserName = notification.UserName,
            Method = notification.Method,
            NewData = JsonSerializer.Serialize(notification.NewManager),
            ObjectId = notification.NewManager.Id,
            ObjectName = nameof(Manager),
            ObjectStoreName = notification.ObjectStoreName
        };

        if (notification.OldManager is not null)
            dto.OldData = JsonSerializer.Serialize(notification.OldManager);

        var integrationEvent = new HistoryCreatedIntegrationEvent(dto);

        await _mediator.Publish(integrationEvent);
    }
}
