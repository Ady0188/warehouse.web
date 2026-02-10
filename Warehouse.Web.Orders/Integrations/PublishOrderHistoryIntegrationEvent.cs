using MediatR;
using System.Text.Json;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Orders.Integrations
{

    internal class PublishOrderHistoryIntegrationEvent : INotificationHandler<OrderHistoryEvent>
    {
        private readonly IMediator _mediator;

        public PublishOrderHistoryIntegrationEvent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(OrderHistoryEvent notification, CancellationToken cancellationToken)
        {
            var dto = new HistoryDto
            {
                StoreName = notification.StoreName,
                UserName = notification.UserName,
                Method = notification.Method,
                NewData = JsonSerializer.Serialize(notification.NewOrder),
                ObjectId = notification.NewOrder.Id,
                ObjectName = nameof(Order),
                ObjectStoreName = notification.ObjectStoreName,
                ObjectAgentName = notification.ObjectAgentName
            };

            if (notification.OldOrder is not null)
                dto.OldData = JsonSerializer.Serialize(notification.OldOrder);

            var integrationEvent = new HistoryCreatedIntegrationEvent(dto);

            await _mediator.Publish(integrationEvent);
        }
    }
}