using FastEndpoints;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Stores.StoreEndpoints;

internal class Delete(IStoreService storeService) : Endpoint<DeleteStoreRequest>
{
    private readonly IStoreService _storeService = storeService;

    public override void Configure()
    {
        Delete(ApiEndpoints.V1.Stores.Delete);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(DeleteStoreRequest req, CancellationToken ct)
    {
        await _storeService.DeleteStoreAsync(req.Id);

        await SendNoContentAsync();
    }
}
