namespace Warehouse.Web.Agents.Endpoints;

public record CreateAgentRequest(string Name, string? Phone, string? Address, long StoreId, string StoreName, long ManagerId, string ManagerName, string? Comment);
