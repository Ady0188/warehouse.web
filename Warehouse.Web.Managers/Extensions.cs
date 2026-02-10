using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using Warehouse.Web.Managers.Endpoints;

namespace Warehouse.Web.Managers;

internal static class Extensions
{
    public static string ToJson(this object o) => System.Text.Json.JsonSerializer.Serialize(o);

    public static GetAllOptions ToOptions(this PagedRequest request) => new GetAllOptions
    {
        Page = request.Page,
        PageSize = request.PageSize,
        SortField = !string.IsNullOrEmpty(request.SortField) ? request.SortField.Trim('+', '-') : request.SortField,
        SortOrder = string.IsNullOrEmpty(request.SortField) ? SortOrder.Unsorted :
                            request.SortField.EndsWith('+') ? SortOrder.Ascending : SortOrder.Descending,
        Filter = string.IsNullOrEmpty(request.Search) ? request.Filter : request.Search
    };

    public static IQueryable<Manager> ApplySorting(this IQueryable<Manager> query, GetAllOptions p)
    {
        if (string.IsNullOrWhiteSpace(p.SortField))
            return query.OrderBy(x => x.Lastname); // сортировка по умолчанию

        var sortMap = new Dictionary<string, Expression<Func<Manager, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Manager.Lastname)] = x => x.Lastname,
            [nameof(Manager.Address)] = x => x.Address,
            [nameof(Manager.Firstname)] = x => x.Firstname,
            [nameof(Manager.Phone)] = x => x.Phone,
            ["StoreName"] = x => x.StoreId,
            [nameof(Manager.CreateDate)] = x => x.CreateDate,
            [nameof(Manager.UpdateDate)] = x => x.UpdateDate
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
    public static IQueryable<Manager> ApplyFilters(this IQueryable<Manager> query, GetAllOptions p)
    {
        query = query.Where(x => x.DeleteDate == null);
        if (string.IsNullOrWhiteSpace(p.Filter))
            return query;

        var handlers = new Dictionary<string, Action<string>>(
            StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Manager.CreateDate)] = v =>
            {
                if (TryParseDate(v, out var startUtc))
                {
                    var endUtc = startUtc.AddDays(1);
                    query = query.Where(x => x.CreateDate >= startUtc && x.CreateDate < endUtc);
                }
            },
            [nameof(Manager.UpdateDate)] = v =>
            {
                if (TryParseDate(v, out var startUtc))
                {
                    var endUtc = startUtc.AddDays(1);
                    query = query.Where(x => x.UpdateDate >= startUtc && x.UpdateDate < endUtc);
                }
            },
            [nameof(Manager.Lastname)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Lastname != null && EF.Functions.ILike(x.Lastname, $"%{v}%"));
            },
            [nameof(Manager.Firstname)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Firstname != null && EF.Functions.ILike(x.Firstname, $"%{v}%"));
            },
            [nameof(Manager.Address)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Address != null && EF.Functions.ILike(x.Address, $"%{v}%"));
            },
            [nameof(Manager.Phone)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Phone != null && EF.Functions.ILike(x.Phone, $"%{v}%"));
            },
            [nameof(Manager.StoreId)] = v =>
            {
                if (long.TryParse(v, out var storeId))
                    query = query.Where(x => x.StoreId == storeId);
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