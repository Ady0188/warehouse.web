using Ardalis.Result;
using MediatR;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Orders.UseCases.Commands
{
    internal record CreateOrderCommand(DateTime Date, long? DocId, long StoreId, long AgentId, decimal Amount, string? Comment, int Type, List<Shared.Requests.OrderAgentRequest>? agents) : IRequest<Result>;
    internal class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;

        public CreateOrderCommandHandler(IOrderRepository orderRepository, IMediator mediator, ICurrentUser currentUser)
        {
            _orderRepository = orderRepository;
            _mediator = mediator;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var storeQuery = new GetStoreByIdQuery(request.StoreId);
                var storeResult = await _mediator.Send(storeQuery);

                if (storeResult.Status == ResultStatus.NotFound)
                    return Result.NotFound($"Store with id '{request.StoreId}' not found");

                var query = new GetAgentByIdQuery(request.AgentId);
                var queryResult = await _mediator.Send(query);

                if (queryResult.Status == ResultStatus.NotFound)
                    return Result.NotFound($"Agent with id '{request.AgentId}' not found");

                if (!Enum.TryParse(request.Type.ToString(), out OrderType type))
                    return Result.Error("Wrong type");

                var order = Order.Create(_currentUser.FullName, _currentUser.StoreName, request.StoreId, queryResult.Value.ManagerId, queryResult.Value.ManagerName, request.AgentId, queryResult.Value.Name, type, request.Amount, request.Date, request.DocId, request.Comment, storeResult.Value.Name);

                if (type == OrderType.AgentRevision && request.agents is not null)
                {
                    foreach (var operAgent in request.agents)
                    {
                        order.AddAgent(operAgent.Id, operAgent.Name, operAgent.Debt, operAgent.Difference, type);
                    }
                }

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Error(ex.Message);
            }
        }
    }
}
