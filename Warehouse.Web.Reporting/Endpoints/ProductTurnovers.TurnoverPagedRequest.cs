using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Reporting.Endpoints;

public class TurnoverPagedRequest
{
    [FromQuery] public int Page { get; set; } = 1;
    [FromQuery] public int PageSize { get; set; } = 10;
    [FromQuery] public long? AgentId { get; set; }
    [FromQuery] public long? ProductId { get; set; }
    [FromQuery] public long? StoreId { get; set; }
    [FromQuery] public string? FromDate { get; set; }
    [FromQuery] public string? ToDate { get; set; }
}