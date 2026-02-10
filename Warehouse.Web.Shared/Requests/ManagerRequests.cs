namespace Warehouse.Web.Shared.Requests;

public record CreateManagerRequest(string Firstname, string Lastname, long StoreId, string? Address, string? Phone);
public record UpdateManagerRequest(string Firstname, string Lastname, long StoreId, string? Address, string? Phone);