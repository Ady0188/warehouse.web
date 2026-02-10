namespace Warehouse.Web.Shared.Requests;
public record CreateAgentRequest(string Name, string? Phone, string? Address, long StoreId, string StoreName, long ManagerId, string ManagerName, string? Comment);
public record UpdateAgentRequest(string Name, string? Phone, string? Address, long StoreId, string StoreName, long ManagerId, string ManagerName, string? Comment);