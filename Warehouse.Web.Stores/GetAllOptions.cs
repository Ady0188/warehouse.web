namespace Warehouse.Web.Stores;

internal class GetAllOptions
{
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
