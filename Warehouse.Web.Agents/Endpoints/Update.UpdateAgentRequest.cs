using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Agents.Endpoints;

public record UpdateAgentRequest([FromRoute]long Id, string Name, string? Phone, string? Address, long StoreId, string StoreName, long ManagerId, string ManagerName, string? Comment);
