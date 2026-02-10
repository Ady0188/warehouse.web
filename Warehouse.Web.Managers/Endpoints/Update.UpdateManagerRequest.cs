using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Managers.Endpoints;

public record UpdateManagerRequest([FromRoute] long Id, string Firstname, string Lastname, long StoreId, string? Address, string? Phone);
