using MediatR;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;
using Ardalis.Result;

namespace Warehouse.Web.Stores.Integrations;
internal class GetStoresByIdsQueryHandler : IRequestHandler<GetStoresByIdsQuery, Result<List<StoreResponse>>>
{
    private readonly IStoreRepository _storeRepository;

    public GetStoresByIdsQueryHandler(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<Result<List<StoreResponse>>> Handle(GetStoresByIdsQuery request, CancellationToken cancellationToken)
    {
        var stores = await _storeRepository.GetByIdsAsync(request.Ids);

        return stores.Select(x => new StoreResponse
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();
    }
}
