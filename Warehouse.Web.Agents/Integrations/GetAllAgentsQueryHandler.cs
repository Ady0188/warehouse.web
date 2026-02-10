using Ardalis.Result;
using MediatR;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Agents.Integrations;

internal class GetAllAgentsQueryHandler : IRequestHandler<GetAllAgentsQuery, Result<List<AgentResponse>>>
{
    private readonly IReadOnlyAgentRepository _agentRepository;
    private readonly IMediator _mediator;

    public GetAllAgentsQueryHandler(IReadOnlyAgentRepository agentRepository, IMediator mediator)
    {
        _agentRepository = agentRepository;
        _mediator = mediator;
    }

    public async Task<Result<List<AgentResponse>>> Handle(GetAllAgentsQuery request, CancellationToken cancellationToken)
    {
        var agents = await _agentRepository.ListAsync();

        var storesQuery = new GetAllStoresQuery(true);
        var storesQueryResult = await _mediator.Send(storesQuery);

        if (!storesQueryResult.IsSuccess)
            return Result.Error(storesQueryResult.Errors.First());

        Dictionary<long, StoreResponse> stores = storesQueryResult.Value
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .ToDictionary(s => s.Id, s => s);

        return agents.Select(a =>
        {

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
        }).ToList();
    }
}
