using MediatR;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;
using Ardalis.Result;
using Warehouse.Web.Managers.Contracts;

namespace Warehouse.Web.Stores.Integrations;

internal class GetAllStoresQueryHandler : IRequestHandler<GetAllStoresQuery, Result<List<StoreResponse>>>
{
    private readonly IReadOnlyStoreRepository _storeRepository;
    private readonly IMediator _mediator;

    public GetAllStoresQueryHandler(IReadOnlyStoreRepository storeRepository, IMediator mediator)
    {
        _storeRepository = storeRepository;
        _mediator = mediator;
    }

    public async Task<Result<List<StoreResponse>>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
    {
        var stores = await _storeRepository.ListAsync();

        if (request.IncludeManager)
        {
            var managerQuery = new GetAllManagersByStoresIdsQuery(stores.Select(x => x.Id).ToArray());
            var managerQueryResult = await _mediator.Send(managerQuery);

            if (managerQueryResult.IsSuccess)
            {
                return stores.Select(x => new StoreResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Managers = managerQueryResult.Value.Items.Where(m => m.StoreId == x.Id).ToList()
                }).ToList();
            }
        }

        return stores.Select(x => new StoreResponse
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();
    }
}
