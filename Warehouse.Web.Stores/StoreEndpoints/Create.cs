using FastEndpoints;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Stores.StoreEndpoints;
internal class Create(IStoreService storeService) : Endpoint<CreateStoreRequest, StoreDto>
{
    private readonly IStoreService _storeService = storeService;

    public override void Configure()
    {
        Post(ApiEndpoints.V1.Stores.Create);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(CreateStoreRequest req, CancellationToken ct)
    {
        var newStore = new StoreDto(default, req.Name);

        await _storeService.CreateStoreAsync(newStore);

        await SendCreatedAtAsync<GetById>(new { newStore.Id }, newStore);
    }
}
