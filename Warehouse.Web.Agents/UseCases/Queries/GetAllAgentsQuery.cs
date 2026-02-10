using Ardalis.Result;
using MediatR;
using Warehouse.Web.Reporting.Contracts;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Agents.UseCases.Queries;

internal record GetAllAgentsQuery(long StoreId, GetAllOptions Options) : IRequest<Result<AgentsResponse>>;
internal class GetAllAgentsQueryHandler : IRequestHandler<GetAllAgentsQuery, Result<AgentsResponse>>
{
    private readonly IReadOnlyAgentRepository _agentRepository;
    private readonly IMediator _mediator;

    public GetAllAgentsQueryHandler(IReadOnlyAgentRepository agentRepository, IMediator mediator)
    {
        _agentRepository = agentRepository;
        _mediator = mediator;
    }

    public async Task<Result<AgentsResponse>> Handle(GetAllAgentsQuery request, CancellationToken cancellationToken)
    {
        var list = await _agentRepository.ListAsync(request.Options);
        var agents = list.Result;

        Dictionary<long, StoreResponse> stores = new();

        if (request.StoreId == 0)
        {
            var storesQuery = new GetAllStoresQuery(true);
            var storesQueryResult = await _mediator.Send(storesQuery);

            if (!storesQueryResult.IsSuccess)
                return Result.Error(storesQueryResult.Errors.First());

            stores = storesQueryResult.Value
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .ToDictionary(s => s.Id, s => s);
        }
        else
        {
            var storesQuery = new GetStoreByIdQuery(request.StoreId, true);
            var storesQueryResult = await _mediator.Send(storesQuery);

            if (!storesQueryResult.IsSuccess)
                return Result.Error(storesQueryResult.Errors.First());

            stores = new Dictionary<long, StoreResponse> { { storesQueryResult.Value.Id, storesQueryResult.Value } };
        }

        if (request.Options.Filter is not null && request.Options.Filter.Contains("(StoreId,"))
        {
            var f = request.Options.Filter.Split("and").First(x => x.StartsWith("(StoreId,"));
            var sId = long.Parse(f.Replace("(StoreId,", string.Empty).Replace(")", string.Empty).Trim());

            var _stores = stores.Where(x => x.Key == sId).ToDictionary();
            var managersIds = _stores.Values.SelectMany(x => x.Managers).Select(x => x.Id);

            agents = agents.Where(x => managersIds.Contains(x.ManagerId)).ToList();
        }
        else if (request.StoreId != 0)
        {
            stores = stores.Where(x => x.Key == request.StoreId).ToDictionary();
            var managersIds = stores.Values.SelectMany(x => x.Managers).Select(x => x.Id);

            agents = agents.Where(x => managersIds.Contains(x.ManagerId)).ToList();
        }

        if (request.Options.IncludeDebts)
        {
            var debtsQuery = new GetAgentsDebtsQuery(request.Options.DateFrom, request.Options.DateTo);
            var debtsQueryResult = await _mediator.Send(debtsQuery);

            if (!debtsQueryResult.IsSuccess)
                return Result.Error(debtsQueryResult.Errors.First());

            var debtsResult = debtsQueryResult.Value.Where(x => agents.Select(a => a.Id).Contains(x.Id)).ToList();

            return new AgentsResponse
            {
                Total = agents.Count(),
                TotalCredit = debtsResult.Where(x => x.DebtOnEnd > 0).Sum(x => x.DebtOnEnd),
                TotalDebt = debtsResult.Where(x => x.DebtOnEnd < 0).Sum(x => x.DebtOnEnd),
                Stores = stores.Values.OrderBy(x => x.Name).ToList(),
                Agents = agents.Select(x => new AgentResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    ManagerId = x.ManagerId
                }).ToList(),
                Items = agents
                .Skip(request.Options.Skip)
                .Take(request.Options.PageSize)
                .Select(a =>
                {

                    var store = stores.Values.FirstOrDefault(x => x.Managers.Any(m => m.Id == a.ManagerId));
                    var manager = store?.Managers.First(m => m.Id == a.ManagerId);
                    var debts = debtsResult.FirstOrDefault(x => x.Id == a.Id) ?? new AgentDebtsResponse();

                    return new AgentResponse
                    {
                        Id = a.Id,
                        Address = a.Address,
                        Comment = a.Comment,
                        ManagerId = a.ManagerId,
                        Name = a.Name,
                        Phone = a.Phone,
                        ManagerName = manager is not null ? $"{manager.Lastname} {manager.Firstname}".Trim() : string.Empty,
                        ManagerPhone = manager is not null ? manager.Phone : string.Empty,
                        StoreId = store is not null ? store.Id : 0,
                        StoreName = store is not null ? store.Name : string.Empty,
                        Debts = debts
                    };
                }).ToList()
            };
        }

        return new AgentsResponse
        {
            Total = agents.Count(),
            Stores = stores.Values.ToList(),
            Items = agents
                .Skip(request.Options.Skip)
                .Take(request.Options.PageSize)
                .Select(a => {

                var store = stores.Values.FirstOrDefault(x => x.Managers.Any(m => m.Id == a.ManagerId));
                var manager = store?.Managers.First(m => m.Id == a.ManagerId);

                return new AgentResponse
                {
                    Id = a.Id,
                    Address = a.Address,
                    Comment = a.Comment,
                    ManagerId = a.ManagerId,
                    Name = a.Name,
                    Phone = a.Phone,
                    ManagerName = manager is not null ? $"{manager.Lastname} {manager.Firstname}".Trim() : string.Empty,
                    StoreId = store is not null ? store.Id : 0,
                    StoreName = store is not null ? store.Name : string.Empty
                };
            }).ToList()
        };
    }
}
