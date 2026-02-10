using FastEndpoints;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Stores.StoreEndpoints;

internal class Update(IStoreService storeService) : Endpoint<UpdateStoreRequest, StoreDto>
{
    private readonly IStoreService _storeService = storeService;
    public override void Configure()
    {
        Put(ApiEndpoints.V1.Stores.Update);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(UpdateStoreRequest req, CancellationToken ct)
    {
        var store = new StoreDto(req.Id, req.Name);

        await _storeService.UpdateStoreAsync(store);
    }
}
