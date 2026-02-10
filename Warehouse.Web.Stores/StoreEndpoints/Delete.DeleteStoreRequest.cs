using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Stores.StoreEndpoints;

public class DeleteStoreRequest
{
    [FromRoute] public long Id { get; set; }
}
