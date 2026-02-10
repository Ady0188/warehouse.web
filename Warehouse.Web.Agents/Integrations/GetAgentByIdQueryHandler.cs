using Ardalis.Result;
using MediatR;
using Warehouse.Web.Agents.Contracts;
using Warehouse.Web.Managers.Contracts;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Agents.Integrations;

internal class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, Result<AgentResponse>>
{
    private readonly IReadOnlyAgentRepository _agentRepository;
    private readonly IMediator _mediator;

    public GetAgentByIdQueryHandler(IReadOnlyAgentRepository agentRepository, IMediator mediator)
    {
        _agentRepository = agentRepository;
        _mediator = mediator;
    }

    public async Task<Result<AgentResponse>> Handle(GetAgentByIdQuery request, CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetByIdAsync(request.Id);

        if (agent is null)
            return Result.NotFound($"Agent with id '{request.Id}' not found");

        var query = new GetManagerByIdQuery(agent.ManagerId);
        var queryResult = await _mediator.Send(query);

        if (queryResult.Status == ResultStatus.NotFound)
            return Result.NotFound($"Manager with id '{agent.ManagerId}' not found");

        var manager = queryResult.Value;

        return new AgentResponse
        {
            Id = agent.Id,
            Address = agent.Address,
            Comment = agent.Comment,
            ManagerId = agent.ManagerId,
            Name = agent.Name,
            Phone = agent.Phone,
            ManagerName = $"{manager.Lastname} {manager.Firstname}".Trim(),
            ManagerPhone = manager.Phone
        };
    }
}
