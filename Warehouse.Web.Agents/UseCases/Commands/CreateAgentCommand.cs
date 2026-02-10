using Ardalis.Result;
using MediatR;
using Warehouse.Web.Managers.Contracts;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Agents.UseCases.Commands;

internal record CreateAgentCommand(string Name, string? Phone, string? Address, long StoreId, long ManagerId, string? Comment) : IRequest<Result>;
internal class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, Result>
{
    private readonly IAgentRepository _agentRepository;
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public CreateAgentCommandHandler(IAgentRepository agentRepository, IMediator mediator, ICurrentUser currentUser)
    {
        _agentRepository = agentRepository;
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var query = new GetManagerByIdQuery(request.ManagerId);
        var queryResult = await _mediator.Send(query);

        if (queryResult.Status == ResultStatus.NotFound)
            return Result.NotFound($"Manager with id '{request.ManagerId}' not found");

        string storeName = queryResult.Value.StoreName;
        string managerName = $"{queryResult.Value.Lastname} {queryResult.Value.Firstname}".Trim();

        var agent = Agent.Create(_currentUser.FullName, _currentUser.StoreName, request.Name, request.ManagerId, request.Address, request.Phone, request.Comment, storeName, managerName);

        await _agentRepository.AddAsync(agent);
        await _agentRepository.SaveChangesAsync();

        return Result.Success();
    }
}
