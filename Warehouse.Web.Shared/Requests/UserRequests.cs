namespace Warehouse.Web.Shared.Requests;

public record CreateUserRequest(string Firstname, string Lastname, long StoreId, string StoreName, string? Address, string? Phone, string Login, string Role);
public record UpdateUserRequest(string Firstname, string Lastname, long StoreId, string StoreName, string? Address, string? Phone);
public class ChangePasswordRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}