using Ardalis.Result;
using MediatR;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Agents.Integrations;

internal class GetAllAgentsByStoreIdQueryHandler : IRequestHandler<GetAllAgentsByStoreIdQuery, Result<List<AgentResponse>>>
{
    private readonly IReadOnlyAgentRepository _agentRepository;
    private readonly IMediator _mediator;

    public GetAllAgentsByStoreIdQueryHandler(IReadOnlyAgentRepository agentRepository, IMediator mediator)
    {
        _agentRepository = agentRepository;
        _mediator = mediator;
    }
    public async Task<Result<List<AgentResponse>>> Handle(GetAllAgentsByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var agents = await _agentRepository.ListAsync();

        var storesQuery = new GetStoreByIdQuery(request.StoreId, true);
        var storesQueryResult = await _mediator.Send(storesQuery);

        if (!storesQueryResult.IsSuccess)
            return Result.Error(storesQueryResult.Errors.First());

        var store = storesQueryResult.Value;

        var managersIds = store.Managers.Select(x => x.Id);

        agents = agents.Where(x => managersIds.Contains(x.ManagerId)).ToList();

        return agents.Select(a => {

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
