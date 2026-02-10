using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Catalog.Endpoints;

public class PagedRequest
{
    [FromQuery] public int Page { get; set; } = 1;
    [FromQuery] public int PageSize { get; set; } = 10;
    [FromQuery] public string? SortField { get; set; }
    [FromQuery] public string? Filter { get; set; }
    [FromQuery] public string? Search { get; set; }
    [FromQuery] public bool? IncluedeRemains { get; set; }
}
