using Ardalis.Result;
using MediatR;
using System.Linq;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Operations.Endpoints;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Operations.UseCases.Commands;

internal record UpdateOperationCommand(long Id, DateTime Date, decimal Amount, decimal Discount, int Type, long ParentId, long StoreId, long ReceiverId, string? Comment, List<OperationProductRequest> Products) : IRequest<Result>;
internal class UpdateOperationCommandHandler : IRequestHandler<UpdateOperationCommand, Result>
{
    private readonly IOperationRepository _operationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public UpdateOperationCommandHandler(IOperationRepository operationRepository, ICurrentUser currentUser, IMediator mediator)
    {
        _operationRepository = operationRepository;
        _currentUser = currentUser;
        _mediator = mediator;
    }
    public async Task<Result> Handle(UpdateOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _operationRepository.GetByIdAsync(request.Id);
        Operation? receiveToUpdate = null;

        if (operation == null)
            return Result.NotFound();

        if (!Enum.TryParse(request.Type.ToString(), out OperationType type))
            return Result.Error("Wrong type");

        if (request.ParentId != 0 && type == OperationType.Receive)
        {
            var parent = await _operationRepository.GetByIdAsync(request.ParentId);

            if (parent is null)
                return Result.NotFound($"Parent operation with id '{request.ParentId}' not found");
        }

        var oldStoreQuery = new GetStoreByIdQuery(operation.StoreId);
        var oldStoreResult = await _mediator.Send(oldStoreQuery);

        var storeQuery = new GetStoreByIdQuery(request.StoreId);
        var storeResult = await _mediator.Send(storeQuery);

        if (storeResult.Status == ResultStatus.NotFound)
            return Result.NotFound($"Store with id '{request.StoreId}' not found");

        var _oldAgent = new AgentResponse();
        var _agent = new AgentResponse();
        var _toStore = new StoreResponse();

        if (type == OperationType.Send || type == OperationType.Receive)
        {
            receiveToUpdate = await _operationRepository.GetByParentIdAsync(operation.Id);
            var toStoreQuery = new GetStoreByIdQuery(request.ReceiverId);
            var toStoreResult = await _mediator.Send(toStoreQuery);

            if (request.ReceiverId != 0 && storeResult.Status == ResultStatus.NotFound)
                return Result.NotFound($"ToStore with id '{request.ReceiverId}' not found");

            _toStore = toStoreResult.Value;
        }
        else
        {
            var oldQuery = new GetAgentByIdQuery(request.ReceiverId);
            var oldQueryResult = await _mediator.Send(oldQuery);

            var query = new GetAgentByIdQuery(request.ReceiverId);
            var queryResult = await _mediator.Send(query);

            if (request.ReceiverId != 0 && queryResult.Status == ResultStatus.NotFound)
                return Result.NotFound($"Agent with id '{request.ReceiverId}' not found");

            _oldAgent = oldQueryResult.Value ?? new AgentResponse();
            _agent = queryResult.Value ?? new AgentResponse();
        }

        long managerId = 0;
        string managerName = string.Empty;
        string managerPhone = string.Empty;

        long agentId = _agent.Id;
        string oldAgentName = string.Empty;
        string agentName = string.Empty;
        string agentPhone = string.Empty;
        string agentAddress = string.Empty;

        if (agentId != 0)
        {
            oldAgentName = _oldAgent.Name;
            agentName = _agent.Name;
            agentPhone = _agent.Phone;
            agentAddress = _agent.Address;
            managerId = _agent.ManagerId;
            managerName = _agent.ManagerName;
            managerPhone = _agent.ManagerPhone;
        }

        var incomingProducts = request.Products ?? Enumerable.Empty<OperationProductRequest>();
        var incomingIds = incomingProducts.Select(p => p.ProductId).ToList();

        var toRemove = operation.Products
                                .Where(p => !incomingIds.Contains(p.ProductId))
                                .ToList();

        foreach (var prod in toRemove)
        {
            operation.RemoveProduct(prod.ProductId);
        }


        var _type = type == OperationType.Receive ? operation.Type : type;
        var _storeName = type == OperationType.Receive ? storeResult.Value.Name : _currentUser.StoreName;

        operation.Update(_currentUser.FullName, _storeName, request.Date, request.Amount, request.Discount, _type, request.ParentId, request.StoreId, _toStore.Id, _toStore.Name, managerId, managerName, managerPhone, agentId, agentName, agentPhone, agentAddress, request.Comment, $"{oldStoreResult.Value.Name}|{_storeName}", oldAgentName);

        var existingIds = operation.Products.Select(p => p.ProductId).ToHashSet();
        foreach (var item in incomingProducts)
        {
            if (existingIds.Contains(item.ProductId))
            {
                operation.UpdateProduct(item.ProductId, item.Code, item.Name, item.Price, item.BuyPrice, item.SellPrice, item.Quantity, item.Manufacturer, item.Unit, item.Difference, type);
            }
            else
            {
                operation.AddProduct(item.ProductId, item.Code, item.Name, item.Price, item.BuyPrice, item.SellPrice, item.Quantity, item.Manufacturer, item.Unit, item.Difference, type);
            }
        }

        if (type == OperationType.Receive)
        {
            var receive = Operation.Create(_currentUser.FullName, _storeName, DateTime.Now, request.Amount, request.Discount, type, request.ParentId, request.StoreId, _toStore.Id, _toStore.Name, managerId, managerName, managerPhone, agentId, agentName, agentPhone, agentAddress, request.Comment, _storeName);

            foreach (var product in incomingProducts)
            {
                receive.AddProduct(product.ProductId, product.Code, product.Name, product.Price, product.BuyPrice, product.SellPrice, product.Quantity, product.Manufacturer, product.Unit, product.Difference, type);
            }

            await _operationRepository.AddAsync(receive);
        }
        else if (type == OperationType.Send && receiveToUpdate is not null)
        {
            receiveToUpdate.Update(_currentUser.FullName, _storeName, request.Date, request.Amount, request.Discount, receiveToUpdate.Type, receiveToUpdate.ParentId, request.StoreId, _toStore.Id, _toStore.Name, managerId, managerName, managerPhone, agentId, agentName, agentPhone, agentAddress, request.Comment, $"{oldStoreResult.Value.Name}|{_storeName}", oldAgentName);

            foreach (var prod in toRemove)
            {
                receiveToUpdate.RemoveProduct(prod.ProductId);
            }


            existingIds = receiveToUpdate.Products.Select(p => p.ProductId).ToHashSet();
            foreach (var item in incomingProducts)
            {
                if (existingIds.Contains(item.ProductId))
                {
                    receiveToUpdate.UpdateProduct(item.ProductId, item.Code, item.Name, item.Price, item.BuyPrice, item.SellPrice, item.Quantity, item.Manufacturer, item.Unit, item.Difference, type);
                }
                else
                {
                    receiveToUpdate.AddProduct(item.ProductId, item.Code, item.Name, item.Price, item.BuyPrice, item.SellPrice, item.Quantity, item.Manufacturer, item.Unit, item.Difference, type);
                }
            }

            await _operationRepository.UpdateAsync(receiveToUpdate);
        }

        await _operationRepository.UpdateAsync(operation);
        await _operationRepository.SaveChangesAsync();

        return Result.Success();
    }
}
