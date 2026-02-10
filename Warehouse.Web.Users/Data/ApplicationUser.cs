using Microsoft.AspNetCore.Identity;

namespace Warehouse.Web.Users.Data;

public class ApplicationUser : IdentityUser
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string? Address { get; set; }
    public long StoreId { get; set; }
    public string StoreName { get; set; }
}
