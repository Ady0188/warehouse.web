using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Users.UserEndpoints;

public record UpdateUserRequest([FromRoute] string Id, string Firstname, string Lastname, long StoreId, string StoreName, string? Address, string? Phone);
