using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Stores.StoreEndpoints;

public class UpdateStoreRequest
{
    [FromRoute] public long Id { get; set; }
    public required string Name { get; set; }
}
