using Ardalis.GuardClauses;
using FastEndpoints;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Stores.StoreEndpoints;
internal class GetById(IStoreService storeService) : Endpoint<GetStoreByIdRequest, StoreResponse>
{
    private readonly IStoreService _storeService = storeService;
    public override void Configure()
    {
        Get(ApiEndpoints.V1.Stores.GetById);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(GetStoreByIdRequest req, CancellationToken ct)
    {
        try
        {
            var store = await _storeService.GetStoreByIdAsync(req.Id);

            if (store is null)
            {
                await SendNotFoundAsync();
                return;
            }

            await SendAsync(new StoreResponse
            {
                Id = store.Id,
                Name = store.Name
            });
        }
        catch (NotFoundException)
        {
            await SendNotFoundAsync();
        }
        catch (Exception)
        {

            throw;
        }
    }
}
