namespace Warehouse.Web.Users.UserEndpoints;

public record ChangePassRequest(string Username, string Password, string ConfirmPassword);
