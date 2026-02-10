using Ardalis.Result;
using MediatR;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Managers.UseCases.Commands;
public record UpdateManagerCommand(string UserId, long UserStoreId, long Id, string Firstname, string Lastname, long StoreId, string? Address, string? Phone) : IRequest<Result>;
internal class UpdateManagerCommandHandler : IRequestHandler<UpdateManagerCommand, Result>
{
    private readonly IManagerRepository _managerRepository;
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public UpdateManagerCommandHandler(IManagerRepository managerRepository, IMediator mediator, ICurrentUser currentUser)
    {
        _managerRepository = managerRepository;
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
    {
        var manager = await _managerRepository.GetByIdAsync(request.Id);

        if (manager is null)
            return Result.NotFound($"Manager with id '{request.Id}' not found");

        var oldStoreQuery = new GetStoreByIdQuery(manager.StoreId);
        var oldStoreQueryResult = await _mediator.Send(oldStoreQuery);

        var storeQuery = new GetStoreByIdQuery(request.StoreId);
        var storeQueryResult = await _mediator.Send(storeQuery);

        if (storeQueryResult.Status == ResultStatus.NotFound)
            return Result.NotFound();

        manager.Update(_currentUser.FullName, _currentUser.StoreName, request.Firstname, request.Lastname, request.StoreId, request.Address, request.Phone, $"{oldStoreQueryResult.Value.Name}|{storeQueryResult.Value.Name}");

        await _managerRepository.UpdateAsync(manager);
        await _managerRepository.SaveChangesAsync();

        return Result.Success();
    }
}
