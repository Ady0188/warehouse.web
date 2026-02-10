using Warehouse.Web.Users.UserEndpoints;

namespace Warehouse.Web.Users;

internal static class Extensions
{
    public static GetAllOptions ToOptions(this PagedRequest request) => new GetAllOptions
    {
        Page = request.Page,
        PageSize = request.PageSize,
        SortField = !string.IsNullOrEmpty(request.SortField) ? request.SortField.Trim('+', '-') : request.SortField,
        SortOrder = string.IsNullOrEmpty(request.SortField) ? SortOrder.Unsorted :
                           request.SortField.EndsWith('+') ? SortOrder.Ascending : SortOrder.Descending,
        Filter = string.IsNullOrEmpty(request.Search) ? request.Filter : request.Search
    };
}
