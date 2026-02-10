using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Warehouse.Web.Catalog;

internal class GetAllOptions
{
    public bool IncluedeRemains { get; set; }
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
