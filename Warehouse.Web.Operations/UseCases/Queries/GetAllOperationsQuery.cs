using Ardalis.Result;
using MediatR;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Catalog.Contracts;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Operations.UseCases.Queries;

internal record GetAllOperationsQuery(long StoreId, OperationType OperationType, GetAllOptions Options) : IRequest<Result<OperationsResponse>>;
internal class GetAllOperationsQueryHandler : IRequestHandler<GetAllOperationsQuery, Result<OperationsResponse>>
{
    private readonly IReadOnlyOperationRepository _operationRepository;
    private readonly IMediator _mediator;

    public GetAllOperationsQueryHandler(IReadOnlyOperationRepository operationRepository, IMediator mediator)
    {
        _operationRepository = operationRepository;
        _mediator = mediator;
    }
    public async Task<Result<OperationsResponse>> Handle(GetAllOperationsQuery request, CancellationToken cancellationToken)
    {
        var list = await _operationRepository.ListWithTotalsAsync(request.Options);
        var operations = list.Result;

        var sendsNotResived = await _operationRepository.ListSendsNotRecivedByToStoreIdAsync(request.StoreId);

        var storesQuery = new GetAllStoresQuery(true);
        var storesQueryResult = await _mediator.Send(storesQuery);

        if (!storesQueryResult.IsSuccess)
            return Result.Error(storesQueryResult.Errors.First());

        var managers = storesQueryResult.Value.SelectMany(x => x.Managers);
        long managerId = 0;
        if (request.Options.Filter is not null && request.Options.Filter.Contains("ManagerId"))
        {
            var managerFilter = request.Options.Filter.Split(")and(").FirstOrDefault(x => x.Contains("ManagerId"));
            if (!string.IsNullOrEmpty(managerFilter))
            {
                var splited = managerFilter.Trim('(', ')').Split(',');
                var idStr = splited.Length > 1 ? Uri.UnescapeDataString(splited[1]?.Trim() ?? string.Empty) : string.Empty;

                if (long.TryParse(idStr, out var parsedId))
                    managerId = parsedId;
            }
        }

        Dictionary<long, StoreResponse>  stores = storesQueryResult.Value
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .ToDictionary(s => s.Id, s => s);

        Dictionary<long, StoreResponse> toStores = stores.ToDictionary(x => x.Key, x => new StoreResponse
        {
            Id = x.Value.Id,
            Name = x.Value.Name,
            Managers = x.Value.Managers
        });

        Dictionary<long, AgentResponse> agents = new();

        if (request.StoreId == 0)
        {
            var agentsQuery = new GetAllAgentsQuery();
            var agentsQueryResult = await _mediator.Send(agentsQuery);

            if (!agentsQueryResult.IsSuccess)
                return Result.Error(agentsQueryResult.Errors.First());

            var _agents = managerId != 0 ? agentsQueryResult.Value.Where(x => x.ManagerId == managerId).ToList() : agentsQueryResult.Value;
            agents = _agents
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .ToDictionary(a => a.Id, a => a);
        }
        else
        {
            var _store = stores.First(s => s.Key == request.StoreId);

            stores = new Dictionary<long, StoreResponse> { { _store.Key, _store.Value } };

            var agentsQuery = new GetAllAgentsByStoreIdQuery(request.StoreId);
            var agentsQueryResult = await _mediator.Send(agentsQuery);

            if (!agentsQueryResult.IsSuccess)
                return Result.Error(agentsQueryResult.Errors.First());

            var _agents = managerId != 0 ? agentsQueryResult.Value.Where(x => x.ManagerId == managerId).ToList() : agentsQueryResult.Value;
            agents = _agents
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .ToDictionary(a => a.Id, a => a);
        }

        if (request.OperationType != OperationType.Send && request.OperationType != OperationType.Receive)
        {
            var agentsIds = agents.Select(x => x.Value.Id).ToList();
            operations = operations.Where(x => agentsIds.Contains(x.AgentId)).ToList();
        }

        var remainsQuery = new GetProductsRemainsQuery(stores.Keys.ToArray());
        var remainsQueryResult = await _mediator.Send(remainsQuery);

        if (!remainsQueryResult.IsSuccess)
            return Result.Error(remainsQueryResult.Errors.First());

        return new OperationsResponse
        {
            Total = operations.Count(),
            TotalProductCount = operations.Sum(x => (long)x.Products.Sum(p => p.Quantity)),
            TotalProductAmount = operations.Sum(x => x.Amount),
            TotalProductDiscount = operations.Sum(x => x.Amount * x.Discount / 100),
            TotalProductToPay = operations.Sum(x => x.Amount - (x.Amount * x.Discount / 100)),
            Stores = stores.Values.ToList(),
            ToStores = toStores.Values.ToList(),
            Agents = agents.Values.ToList(),
            Products = remainsQueryResult.Value.Items,
            NotResivedItems = sendsNotResived
                .Select(a =>
                {
                    var store = toStores.Values.FirstOrDefault(x => x.Id == a.StoreId);
                    var toStore = toStores.Values.FirstOrDefault(x => x.Id == a.ToStoreId);
                    var agent = agents.Values.FirstOrDefault(m => m.Id == a.AgentId);

                    return new OperationResponse
                    {
                        Id = a.Id,
                        Code = a.Code,
                        Date = a.Date,
                        StoreId = store is not null ? store.Id : 0,
                        StoreName = store is not null ? store.Name : string.Empty,
                        ToStoreId = toStore is not null ? toStore.Id : 0,
                        ToStoreName = toStore is not null ? toStore.Name : string.Empty,
                        AgentId = agent is not null ? agent.Id : 0,
                        AgentName = agent is not null ? agent.Name : string.Empty,
                        Amount = a.Amount,
                        Comment = a.Comment,
                        Type = (int)a.Type,
                        DiscountPercentage = a.Discount,
                        Products = a.Products.Select(p => new OperationProductResponse
                        {
                            Id = p.Id,
                            ProductId = p.ProductId,
                            ProductName = p.Name,
                            Quantity = p.Quantity,
                            Price = p.Price,
                            BuyPrice = p.BuyPrice,
                            SellPrice = p.SellPrice,
                            Unit = p.Unit,
                            Manufacturer = p.Manufacturer,
                            Difference = p.Difference
                        }).ToList(),
                    };
                }).ToList(),
            Items = operations
                .Skip(request.Options.Skip)
                .Take(request.Options.PageSize)
                .Select(a =>
                {
                    var store = toStores.Values.FirstOrDefault(x => x.Id == a.StoreId);
                    var toStore = toStores.Values.FirstOrDefault(x => x.Id == a.ToStoreId);
                    var agent = agents.Values.FirstOrDefault(m => m.Id == a.AgentId);
                    var manager = managers.FirstOrDefault(m => m.Id == (agent is null ? -1 : agent.ManagerId));

                    return new OperationResponse
                    {
                        Id = a.Id,
                        Code = a.Code,
                        Date = a.Date,
                        StoreId = store is not null ? store.Id : 0,
                        StoreName = store is not null ? store.Name : string.Empty,
                        ToStoreId = toStore is not null ? toStore.Id : 0,
                        ToStoreName = toStore is not null ? toStore.Name : string.Empty,
                        AgentId = agent is not null ? agent.Id : 0,
                        AgentName = agent is not null ? agent.Name : string.Empty,
                        ManagerName = manager is not null ? $"{manager.Lastname} {manager.Firstname}".Trim() : string.Empty,
                        Amount = a.Amount,
                        Comment = a.Comment,
                        Type = (int)a.Type,
                        IsReceived = a.IsReceived,
                        DiscountPercentage = a.Discount,
                        Products = a.Products.Select(p => new OperationProductResponse
                        {
                            Id = p.Id,
                            ProductId = p.ProductId,
                            ProductName = p.Name,
                            Quantity = p.Quantity,
                            Price = p.Price,
                            BuyPrice = p.BuyPrice,
                            SellPrice = p.SellPrice,
                            Unit = p.Unit,
                            Manufacturer = p.Manufacturer,
                            Difference = p.Difference
                        }).ToList(),
                    };
                }).ToList()
        };
    }
}

