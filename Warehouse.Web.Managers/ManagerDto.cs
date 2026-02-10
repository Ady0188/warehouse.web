namespace Warehouse.Web.Managers;

internal record ManagerDto(long Id, string Firstname, string Lastname, long StoreId, DateTime CreateDate, DateTime UpdateDate, DateTime DeleteDate, string? Address, string? Phone);