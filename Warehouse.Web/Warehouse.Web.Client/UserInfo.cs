namespace Warehouse.Web.Client
{
    // Add properties to this class and update the server and client AuthenticationStateProviders
    // to expose more information about the authenticated user to the client.
    public class UserInfo
    {
        public required string UserId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required List<string> Roles { get; set; }
        public required string StoreId { get; set; }
        public required string StoreName { get; set; }
    }
}
