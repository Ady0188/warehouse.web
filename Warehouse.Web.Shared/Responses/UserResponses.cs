using System.Net;

namespace Warehouse.Web.Shared.Responses;
public class UserResponse
{
    public string Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public long StoreId { get; set; }
    public string StoreName { get; set; }
    public string Login { get; set; }
    public string? Email { get; set; }
}

public class UsersResponse
{
    public List<StoreResponse> Stores { get; set; } = new List<StoreResponse>();
    public List<UserResponse> Items { get; set; } = new List<UserResponse>();
    public int Total { get; set; }
}
