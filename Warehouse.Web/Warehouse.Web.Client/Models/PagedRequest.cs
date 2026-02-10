namespace Warehouse.Web.Client.Models;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public string? Filter { get; set; }
    public string? Search { get; set; }
    public bool ResetPage { get; set; }
}