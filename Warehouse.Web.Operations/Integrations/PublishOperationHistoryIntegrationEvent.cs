using MediatR;
using System.Text.Json;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Operations.Integrations;

internal class PublishOperationHistoryIntegrationEvent : INotificationHandler<OperationHistoryEvent>
{
    private readonly IMediator _mediator;

    public PublishOperationHistoryIntegrationEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OperationHistoryEvent notification, CancellationToken cancellationToken)
    {
        var dto = new HistoryDto
        {
            StoreName = notification.StoreName,
            UserName = notification.UserName,
            Method = notification.Method,
            NewData = JsonSerializer.Serialize(notification.NewOperation),
            ObjectId = notification.NewOperation.Id,
            ObjectName = nameof(Operation),
            ObjectStoreName = notification.ObjectStoreName,
            ObjectAgentName = notification.ObjectAgentName
        };

        if (notification.OldOperation is not null)
            dto.OldData = JsonSerializer.Serialize(notification.OldOperation);

        var integrationEvent = new HistoryCreatedIntegrationEvent(dto);

        await _mediator.Publish(integrationEvent);
    }
}
