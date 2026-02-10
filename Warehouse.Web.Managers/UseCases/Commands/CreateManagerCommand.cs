using Ardalis.Result;
using MediatR;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Managers.UseCases.Commands;
public record CreateManagerCommand(string Firstname, string Lastname, long StoreId, string? Address, string? Phone) : IRequest<Result>;
internal class CreateManagerCommandHandler : IRequestHandler<CreateManagerCommand, Result>
{
    private readonly IManagerRepository _managerRepository;
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public CreateManagerCommandHandler(IManagerRepository managerRepository, IMediator mediator, ICurrentUser currentUser)
    {
        _managerRepository = managerRepository;
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
    {
        var query = new GetStoreByIdQuery(request.StoreId);
        var queryResult = await _mediator.Send(query);

        if (queryResult.Status == ResultStatus.NotFound)
            return Result.NotFound($"Store with id '{request.StoreId}' not found");

        var exists = await _managerRepository.ExistsByNameAsync(request.Firstname.Trim(), request.Lastname.Trim());

        if (exists)
            return Result.Conflict($"Manager '{request.Firstname} {request.Lastname}' exists");

        var manager = Manager.Create(_currentUser.FullName, _currentUser.StoreName, request.Firstname, request.Lastname, request.StoreId, request.Address, request.Phone, queryResult.Value.Name);

        await _managerRepository.AddAsync(manager);
        await _managerRepository.SaveChangesAsync();

        return Result.Success();
    }
}
