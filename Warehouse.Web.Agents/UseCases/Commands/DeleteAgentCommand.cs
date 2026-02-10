using Ardalis.Result;
using MediatR;

namespace Warehouse.Web.Agents.UseCases.Commands;
internal record DeleteAgentCommand(long Id) : IRequest<Result>;
internal class DeleteAgentCommandHandler : IRequestHandler<DeleteAgentCommand, Result>
{
    private readonly IAgentRepository _agentRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteAgentCommandHandler(IAgentRepository agentRepository, ICurrentUser currentUser)
    {
        _agentRepository = agentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetByIdAsync(request.Id);

        if (agent is null)
        {
            return Result.NotFound();
        }
        else
        {
            agent.Delete(_currentUser.FullName, _currentUser.StoreName);

            //await _agentRepository.DeleteAsync(agent);
            await _agentRepository.UpdateAsync(agent);
            await _agentRepository.SaveChangesAsync();

            return Result.Success();
        }
    }
}