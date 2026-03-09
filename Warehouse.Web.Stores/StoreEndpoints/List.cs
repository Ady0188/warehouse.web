using FastEndpoints;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Stores.StoreEndpoints;

internal class List(IStoreService storeService) : Endpoint<PagedRequest, StoresResponse>
{
    private readonly IStoreService _storeService = storeService;
    public override void Configure()
    {
        Get(ApiEndpoints.V1.Stores.GetAll);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        var user = User;
        var list = await _storeService.ListStoresAsync(request.ToOptions());
        var stores = list.Result;

        await SendAsync(new StoresResponse
        {
            Total = list.Total,
            Items = stores.Select(x => new StoreResponse
            {
                Id = x.Id,
                Name = x.Name
            }).ToList()
        });
    }
}
