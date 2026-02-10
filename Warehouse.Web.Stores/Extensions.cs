using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using Warehouse.Web.Stores.StoreEndpoints;

namespace Warehouse.Web.Stores;

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

    public static IQueryable<Store> ApplySorting(this IQueryable<Store> query, GetAllOptions p)
    {
        if (string.IsNullOrWhiteSpace(p.SortField))
            return query.OrderBy(x => x.Name); // сортировка по умолчанию

        var sortMap = new Dictionary<string, Expression<Func<Store, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Store.CreatedDate)] = x => x.CreatedDate,
            [nameof(Store.UpdatedDate)] = x => x.UpdatedDate,
            [nameof(Store.Name)] = x => x.Name
        };

        if (sortMap.TryGetValue(p.SortField, out var expr))
        {
            if (p.SortOrder == SortOrder.Ascending)
                query = query.OrderBy(expr);
            else if (p.SortOrder == SortOrder.Descending)
                query = query.OrderByDescending(expr);
        }

        return query;
    }
    public static IQueryable<Store> ApplyFilters(this IQueryable<Store> query, GetAllOptions p)
    {
        query = query.Where(x => x.DelatedDate == null);

        if (string.IsNullOrWhiteSpace(p.Filter))
            return query;

        var handlers = new Dictionary<string, Action<string>>(
            StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Store.CreatedDate)] = v =>
            {
                if (TryParseDate(v, out var startUtc))
                {
                    var endUtc = startUtc.AddDays(1);
                    query = query.Where(x => x.CreatedDate >= startUtc && x.CreatedDate < endUtc);
                }
            },
            [nameof(Store.UpdatedDate)] = v =>
            {
                if (TryParseDate(v, out var startUtc))
                {
                    var endUtc = startUtc.AddDays(1);
                    query = query.Where(x => x.UpdatedDate >= startUtc && x.UpdatedDate < endUtc);
                }
            },
            [nameof(Store.Name)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Name != null && EF.Functions.ILike(x.Name, $"%{v}%"));
            }
        };

        var filterData = p.Filter;

        foreach (var item in filterData.Split(")and("))
        {
            var fieldValue = item.Trim('(', ')').Split(',');
            if (fieldValue.Length < 2) continue;

            var field = fieldValue[0]?.Trim();
            var value = Uri.UnescapeDataString(fieldValue[1]?.Trim() ?? string.Empty).ToLower();

            if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value))
                continue;

            if (handlers.TryGetValue(field, out var apply))
                apply(value);
        }

        return query.AsNoTracking();

        static bool TryParseDate(string value, out DateTime utcStart)
        {
            utcStart = default;

            if (DateTime.TryParseExact(value, new[] { "ddMMyyyy", "ddMMyy" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                utcStart = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
                return true;
            }
            return false;
        }
    }
}
