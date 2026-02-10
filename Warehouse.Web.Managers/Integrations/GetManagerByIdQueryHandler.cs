using Ardalis.Result;
using MediatR;
using Warehouse.Web.Managers.Contracts;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Managers.Integrations;

internal class GetManagerByIdQueryHandler : IRequestHandler<GetManagerByIdQuery, Result<ManagerResponse>>
{
    private readonly IManagerRepository _managerRepository;
    private readonly IMediator _mediator;

    public GetManagerByIdQueryHandler(IManagerRepository managerRepository, IMediator mediator)
    {
        _managerRepository = managerRepository;
        _mediator = mediator;
    }

    public async Task<Result<ManagerResponse>> Handle(GetManagerByIdQuery request, CancellationToken cancellationToken)
    {
        var manager = await _managerRepository.GetByIdAsync(request.Id);

        if (manager is null)
            return Result.NotFound();

        var storeQuery = new GetStoreByIdQuery(manager.StoreId);
        var storeResult = await _mediator.Send(storeQuery);

        if (storeResult.Status == ResultStatus.NotFound)
            return Result.NotFound($"Store with id '{manager.StoreId}' not found");

        return new ManagerResponse
        {
            Id = manager.Id,
            Address = manager.Address,
            Firstname = manager.Firstname,
            Lastname = manager.Lastname,
            Phone = manager.Phone,
            StoreId = manager.StoreId,
            StoreName = storeResult.Value.Name
        };
    }
}
