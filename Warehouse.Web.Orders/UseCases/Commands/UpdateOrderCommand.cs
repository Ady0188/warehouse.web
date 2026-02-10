using Ardalis.Result;
using MediatR;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Orders.UseCases.Commands
{
    internal record UpdateOrderCommand(long Id, DateTime Date, long? DocId, long StoreId, long AgentId, decimal Amount, string? Comment, int Type) : IRequest<Result>;
    internal class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IMediator _mediator;

        public UpdateOrderCommandHandler(IOrderRepository orderRepository, ICurrentUser currentUser, IMediator mediator)
        {
            _orderRepository = orderRepository;
            _currentUser = currentUser;
            _mediator = mediator;
        }

        public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id);

            if (order == null)
                return Result.NotFound();

            var oldStoreQuery = new GetStoreByIdQuery(order.StoreId);
            var oldStoreResult = await _mediator.Send(oldStoreQuery);

            var storeQuery = new GetStoreByIdQuery(request.StoreId);
            var storeResult = await _mediator.Send(storeQuery);

            if (storeResult.Status == ResultStatus.NotFound)
                return Result.NotFound($"Store with id '{request.StoreId}' not found");

            var oldQuery = new GetAgentByIdQuery(order.AgentId);
            var oldQueryResult = await _mediator.Send(oldQuery);

            var query = new GetAgentByIdQuery(request.AgentId);
            var queryResult = await _mediator.Send(query);

            if (queryResult.Status == ResultStatus.NotFound)
                return Result.NotFound($"Agent with id '{request.AgentId}' not found");

            if (!Enum.TryParse(request.Type.ToString(), out OrderType type))
                return Result.Error("Wrong type");

            order.Update(_currentUser.FullName, _currentUser.StoreName, request.StoreId, queryResult.Value.ManagerId, queryResult.Value.ManagerName, request.AgentId, queryResult.Value.Name, type, request.Amount, request.Date, request.DocId, request.Comment, $"{oldStoreResult.Value.Name}|{storeResult.Value.Name}", oldQueryResult.Value.Name);

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return Result.Success();
        }
    }
}
