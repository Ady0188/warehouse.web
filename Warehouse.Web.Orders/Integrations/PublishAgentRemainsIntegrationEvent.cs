using MediatR;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Orders.Integrations
{
    internal class PublishAgentRemainsIntegrationEvent : INotificationHandler<AgentRemainsEvent>
    {
        private readonly IMediator _mediator;

        public PublishAgentRemainsIntegrationEvent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(AgentRemainsEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Order.AgentId == 0) return;

            int inOrOut = notification.Order.Type == OrderType.Receipt ? 1 : -1;

            var dto = new AgentRemainsDto
            {
                StoreId = notification.Order.StoreId,
                StoreName = notification.StoreName,
                ManagerId = notification.ManagerId,
                ManagerName = notification.ManagerName,
                AgentId = notification.Order.AgentId,
                AgentName = notification.AgentName,
                ObjectId = notification.Order.Id,
                ObjectCode = notification.Order.Code,
                ObjectName = "Order",
                ObjectType = (short)notification.Order.Type,
                Amount = notification.Order.Amount * inOrOut,
                Disctount = 0,
                Date = notification.Order.Date,
                Method = notification.Method
            };

            var integrationEvent = new AgentRemainsIntegrationEvent(dto);

            await _mediator.Publish(integrationEvent);

        }
    }
}