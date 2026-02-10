using MediatR;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;
using Ardalis.Result;
using Warehouse.Web.Managers.Contracts;

namespace Warehouse.Web.Stores.Integrations;

internal class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, Result<StoreResponse>>
{
    private readonly IReadOnlyStoreRepository _storeRepository;
    private readonly IMediator _mediator;

    public GetStoreByIdQueryHandler(IReadOnlyStoreRepository storeRepository, IMediator mediator)
    {
        _storeRepository = storeRepository;
        _mediator = mediator;
    }

    public async Task<Result<StoreResponse>> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        var store = await _storeRepository.GetByIdAsync(request.Id);

        if (store is null)
            return Result.NotFound();

        if (request.IncludeManager)
        {
            var managerQuery = new GetAllManagersByStoresIdsQuery(new long[] { store.Id });
            var managerQueryResult = await _mediator.Send(managerQuery);

            if (managerQueryResult.IsSuccess)
            {
                return new StoreResponse
                {
                    Id = store.Id,
                    Name = store.Name,
                    Managers = managerQueryResult.Value.Items.Where(m => m.StoreId == store.Id).ToList()
                };
            }
        }

        return new StoreResponse
        {
            Id = store.Id,
            Name = store.Name
        };
    }
}
