using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using Warehouse.Web.Agents.Endpoints;

namespace Warehouse.Web.Agents;

internal static class Extensions
{
    public static string ToJson(this object o) => System.Text.Json.JsonSerializer.Serialize(o);

    public static GetAllOptions ToOptions(this PagedRequest request, int pageSize = 0)
    {
        var dateTo = string.IsNullOrEmpty(request.DebtsTo) ? DateTime.Now : DateTime.ParseExact(request.DebtsTo, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);
        var dateFrom = string.IsNullOrEmpty(request.DebtsFrom) ? new DateTime(dateTo.Year, dateTo.Month, 1, 0, 0, 0) : DateTime.ParseExact(request.DebtsFrom, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture);

        return new GetAllOptions
        {
            IncludeDebts = request.IncludeDebts ?? false,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Page = request.Page,
            PageSize = pageSize > 0 ? pageSize : request.PageSize,
            SortField = !string.IsNullOrEmpty(request.SortField) ? request.SortField.Trim('+', '-') : request.SortField,
            SortOrder = string.IsNullOrEmpty(request.SortField) ? SortOrder.Unsorted :
                            request.SortField.EndsWith('+') ? SortOrder.Ascending : SortOrder.Descending,
            Filter = string.IsNullOrEmpty(request.Search) ? request.Filter : request.Search
        };
    }

    public static IQueryable<Agent> ApplySorting(this IQueryable<Agent> query, GetAllOptions p)
    {
        if (string.IsNullOrWhiteSpace(p.SortField))
            return query.OrderBy(x => x.Name); // сортировка по умолчанию

        var sortMap = new Dictionary<string, Expression<Func<Agent, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Agent.Name)] = x => x.Name,
            [nameof(Agent.Phone)] = x => x.Phone,
            [nameof(Agent.Address)] = x => x.Address,
            [nameof(Agent.Comment)] = x => x.Comment,
            [nameof(Agent.ManagerId)] = x => x.ManagerId,
            [nameof(Agent.CreateDate)] = x => x.CreateDate,
            [nameof(Agent.UpdateDate)] = x => x.UpdateDate
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
    public static IQueryable<Agent> ApplyFilters(this IQueryable<Agent> query, GetAllOptions p)
    {
        query = query.Where(x => x.DeleteDate == null);

        if (string.IsNullOrWhiteSpace(p.Filter))
            return query;

        var handlers = new Dictionary<string, Action<string>>(
            StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Agent.CreateDate)] = v =>
            {
                if (TryParseDate(v, out var startUtc))
                {
                    var endUtc = startUtc.AddDays(1);
                    query = query.Where(x => x.CreateDate >= startUtc && x.CreateDate < endUtc);
                }
            },
            [nameof(Agent.UpdateDate)] = v =>
            {
                if (TryParseDate(v, out var startUtc))
                {
                    var endUtc = startUtc.AddDays(1);
                    query = query.Where(x => x.UpdateDate >= startUtc && x.UpdateDate < endUtc);
                }
            },
            [nameof(Agent.Name)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Name != null && EF.Functions.ILike(x.Name, $"%{v}%"));
            },
            [nameof(Agent.Address)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Address != null && EF.Functions.ILike(x.Address, $"%{v}%"));
            },
            [nameof(Agent.Comment)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Comment != null && EF.Functions.ILike(x.Comment, $"%{v}%"));
            },
            [nameof(Agent.Phone)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Phone != null && EF.Functions.ILike(x.Phone, $"%{v}%"));
            },
            ["ManagerName"] = v =>
            {
                if (long.TryParse(v, out var managerId))
                    query = query.Where(x => x.ManagerId == managerId);
            },
            [nameof(Agent.ManagerId)] = v =>
            {
                if (long.TryParse(v, out var managerId))
                    query = query.Where(x => x.ManagerId == managerId);
            },
            [nameof(Agent.Id)] = v =>
            {
                if (long.TryParse(v, out var id))
                    query = query.Where(x => x.Id == id);
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
