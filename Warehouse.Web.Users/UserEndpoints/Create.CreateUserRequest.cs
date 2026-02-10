namespace Warehouse.Web.Users.UserEndpoints;

public record CreateUserRequest(string Firstname, string Lastname, long StoreId, string StoreName, string? Address, string? Phone, string Login, string Role = "User");
