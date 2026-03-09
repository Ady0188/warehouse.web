using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Operations.Endpoints;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Operations.UseCases.Commands;

internal record CreateOperationCommand(DateTime Date, decimal Amount, decimal Discount, int Type, long StoreId, long ReceiverId, string? Comment, List<OperationProductRequest> Products) : IRequest<Result>;
internal class CreateOperationCommandHandler : IRequestHandler<CreateOperationCommand, Result>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateOperationCommandHandler> _logger;

    public CreateOperationCommandHandler(IOperationRepository operationRepository, IMediator mediator, ICurrentUser currentUser, ILogger<CreateOperationCommandHandler> logger)
    {
        _operationRepository = operationRepository;
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }
    public async Task<Result> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var storeQuery = new GetStoreByIdQuery(request.StoreId);
            var storeResult = await _mediator.Send(storeQuery);

            if (storeResult.Status == ResultStatus.NotFound)
                return Result.NotFound($"Store with id '{request.StoreId}' not found");

            if (!Enum.TryParse(request.Type.ToString(), out OperationType type))
                return Result.Error("Wrong type");

            var _agent = new AgentResponse();
            var _toStore = new StoreResponse();

            if (type == OperationType.Send || type == OperationType.Receive)
            {
                var toStoreQuery = new GetStoreByIdQuery(request.ReceiverId);
                var toStoreResult = await _mediator.Send(toStoreQuery);

                if (request.ReceiverId != 0 && toStoreResult.Status == ResultStatus.NotFound)
                    return Result.NotFound($"ToStore with id '{request.ReceiverId}' not found");

                _toStore = toStoreResult.Value;
            }
            else
            {
                var query = new GetAgentByIdQuery(request.ReceiverId);
                var queryResult = await _mediator.Send(query);

                if (request.ReceiverId != 0 && queryResult.Status == ResultStatus.NotFound)
                    return Result.NotFound($"Agent with id '{request.ReceiverId}' not found");

                _agent = queryResult.Value ?? new AgentResponse();
            }

            long managerId = 0;
            string managerName = string.Empty;
            string managerPhone = string.Empty;

            long agentId = _agent.Id;
            string agentName = string.Empty;
            string agentPhone = string.Empty;
            string agentAddress = string.Empty;

            if (agentId != 0)
            {
                agentName = _agent.Name;
                agentPhone = _agent.Phone;
                agentAddress = _agent.Address;
                managerId = _agent.ManagerId;
                managerName = _agent.ManagerName;
                managerPhone = _agent.ManagerPhone;
            }

            var operation = Operation.Create(_currentUser.FullName, _currentUser.StoreName, request.Date, request.Amount, request.Discount, type, 0, request.StoreId, _toStore.Id, _toStore.Name, managerId, managerName, managerPhone, agentId, agentName, agentPhone, agentAddress, request.Comment, storeResult.Value.Name);

            foreach (var product in request.Products)
            {
                operation.AddProduct(product.ProductId, product.Code, product.Name, product.Price, product.BuyPrice, product.SellPrice, product.Quantity, product.Manufacturer, product.Unit, product.Difference, type);
            }

            await _operationRepository.AddAsync(operation);
            await _operationRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create operation (StoreId={StoreId}, ReceiverId={ReceiverId}).", request.StoreId, request.ReceiverId);
            return Result.Error(ex.Message);
        }
    }
}
