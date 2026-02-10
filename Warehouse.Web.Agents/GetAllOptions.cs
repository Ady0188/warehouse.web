namespace Warehouse.Web.Agents;

internal class GetAllOptions
{
    public bool IncludeDebts { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string? SortField { get; set; }
    public SortOrder? SortOrder { get; set; }
    public string? Filter { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public int Skip
    {
        get
        {
            return (Page - 1) * PageSize;
        }
    }
}

public enum SortOrder
{
    Unsorted,
    Ascending,
    Descending
}
