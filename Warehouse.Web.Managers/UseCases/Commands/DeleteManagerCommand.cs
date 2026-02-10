using Ardalis.Result;
using MediatR;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Managers.UseCases.Commands;

public record DeleteManagerCommand(string UserId, long UserStoreId, long Id) : IRequest<Result>;
internal class DeleteManagerCommandHandler : IRequestHandler<DeleteManagerCommand, Result>
{
    private readonly IManagerRepository _managerRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteManagerCommandHandler(IManagerRepository managerRepository, ICurrentUser currentUser)
    {
        _managerRepository = managerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteManagerCommand request, CancellationToken cancellationToken)
    {
        var manager = await _managerRepository.GetByIdAsync(request.Id);

        if (manager is null)
            return Result.NotFound($"Manager with id '{request.Id}' not found");

        manager.Delete(_currentUser.FullName, _currentUser.StoreName);
        
        await _managerRepository.UpdateAsync(manager);
        await _managerRepository.SaveChangesAsync();

        return Result.Success();

        //var manager = await _managerRepository.GetByIdAsync(request.Id);

        //if (manager is null)
        //    return Result.NotFound();

        //manager.Delete(_currentUser.FullName, _currentUser.StoreName);

        //await _managerRepository.DeleteAsync(manager);
        //await _managerRepository.SaveChangesAsync();

        //return Result.Success();
    }
}
