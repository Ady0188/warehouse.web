namespace Warehouse.Web.Managers.Endpoints;

public record CreateManagerRequest(string Firstname, string Lastname, long StoreId, string? Address, string? Phone);
