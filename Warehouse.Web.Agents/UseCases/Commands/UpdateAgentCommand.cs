using Ardalis.Result;
using MediatR;
using Warehouse.Web.Managers.Contracts;

namespace Warehouse.Web.Agents.UseCases.Commands;
internal record UpdateAgentCommand(long Id, string Name, string? Phone, string? Address, long StoreId, long ManagerId, string? Comment) : IRequest<Result>;
internal class UpdateAgentCommandHandler : IRequestHandler<UpdateAgentCommand, Result>
{
    private readonly IMediator _mediator;
    private readonly IAgentRepository _agentRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateAgentCommandHandler(IAgentRepository agentRepository, ICurrentUser currentUser, IMediator mediator)
    {
        _agentRepository = agentRepository;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<Result> Handle(UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetByIdAsync(request.Id);

        if (agent == null)
            return Result.NotFound();

        var oldQuery = new GetManagerByIdQuery(agent.ManagerId);
        var oldQueryResult = await _mediator.Send(oldQuery);

        var query = new GetManagerByIdQuery(request.ManagerId);
        var queryResult = await _mediator.Send(query);

        if (queryResult.Status == ResultStatus.NotFound)
            return Result.NotFound($"Manager with id '{request.ManagerId}' not found");

        string storeName = queryResult.Value.StoreName;
        string managerName = $"{queryResult.Value.Lastname} {queryResult.Value.Firstname}".Trim();

        string oldStoreName = oldQueryResult.Value.StoreName;
        string oldManagerName = $"{oldQueryResult.Value.Lastname} {oldQueryResult.Value.Firstname}".Trim();

        agent.Update(_currentUser.FullName, _currentUser.StoreName, request.Name, request.ManagerId, request.Address, request.Phone, request.Comment, $"{oldStoreName}|{storeName}", $"{oldManagerName}|{managerName}");

        await _agentRepository.UpdateAsync(agent);
        await _agentRepository.SaveChangesAsync();

        return Result.Success();
    }
}
