using MediatR;
using System.Text.Json;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Agents.Integrations;

internal class PublishAgentHistoryIntegrationEvent : INotificationHandler<AgentHistoryEvent>
{
    private readonly IMediator _mediator;

    public PublishAgentHistoryIntegrationEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(AgentHistoryEvent notification, CancellationToken cancellationToken)
    {
        var dto = new HistoryDto
        {
            StoreName = notification.StoreName,
            UserName = notification.UserName,
            Method = notification.Method,
            NewData = JsonSerializer.Serialize(notification.NewAgent),
            ObjectId = notification.NewAgent.Id,
            ObjectName = nameof(Agent),
            ObjectStoreName = notification.ObjectStoreName,
            ObjectManagerName = notification.ObjectManagerName
        };

        if (notification.OldAgent is not null)
            dto.OldData = JsonSerializer.Serialize(notification.OldAgent);

        var integrationEvent = new HistoryCreatedIntegrationEvent(dto);

        await _mediator.Publish(integrationEvent);
    }
}