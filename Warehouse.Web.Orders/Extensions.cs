using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using Warehouse.Web.Orders.Endpoints;

namespace Warehouse.Web.Orders
{
    internal static class Extensions
    {
        public static string GetTitle(this int input) => input switch
        {
            0 => "Приходный ордер",
            1 => "Расходный ордер"
        };

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

        public static IQueryable<Order> ApplySorting(this IQueryable<Order> query, GetAllOptions p)
        {
            if (string.IsNullOrWhiteSpace(p.SortField))
                return query.OrderByDescending(x => x.Id); // сортировка по умолчанию

            var sortMap = new Dictionary<string, Expression<Func<Order, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(Order.Code)] = x => x.Code,
                [nameof(Order.Date)] = x => x.Date,
                [nameof(Order.Amount)] = x => x.Amount,
                [nameof(Order.Comment)] = x => x.Comment,
                [nameof(Order.Type)] = x => x.Type,
                [nameof(Order.CreateDate)] = x => x.CreateDate,
                [nameof(Order.UpdateDate)] = x => x.UpdateDate
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
        public static IQueryable<Order> ApplyFilters(this IQueryable<Order> query, GetAllOptions p)
        {
            if (string.IsNullOrWhiteSpace(p.Filter))
                return query;

            var handlers = new Dictionary<string, Action<string>>(
                StringComparer.OrdinalIgnoreCase)
            {
                [nameof(Order.CreateDate)] = v =>
                {
                    if (TryParseDate(v, out var startUtc))
                    {
                        var endUtc = startUtc.AddDays(1);
                        query = query.Where(x => x.CreateDate >= startUtc && x.CreateDate < endUtc);
                    }
                },
                [nameof(Order.UpdateDate)] = v =>
                {
                    if (TryParseDate(v, out var startUtc))
                    {
                        var endUtc = startUtc.AddDays(1);
                        query = query.Where(x => x.UpdateDate >= startUtc && x.UpdateDate < endUtc);
                    }
                },
                [nameof(Order.Date)] = v =>
                {
                    if (TryParseDate(v, out var startUtc))
                    {
                        var endUtc = startUtc.AddDays(1);
                        query = query.Where(x => x.Date >= startUtc && x.Date < endUtc);
                    }
                },
                ["FromDate"] = v =>
                {
                    if (TryParseDate(v, out var startUtc))
                    {
                        query = query.Where(x => x.Date >= startUtc);
                    }
                },
                ["ToDate"] = v =>
                {
                    if (TryParseDate(v, out var startUtc))
                    {
                        var endUtc = startUtc.AddDays(1);
                        query = query.Where(x => x.Date < endUtc);
                    }
                },
                [nameof(Order.Code)] = v =>
                {
                    if (int.TryParse(v, out var code))
                        query = query.Where(x => x.Code == code);
                },
                [nameof(Order.DocId)] = v =>
                {
                    if (long.TryParse(v, out var docId))
                        query = query.Where(x => x.DocId != null && x.DocId == docId);
                },
                [nameof(Order.StoreId)] = v =>
                {
                    if (long.TryParse(v, out var storeId))
                        query = query.Where(x => x.StoreId == storeId);
                },
                [nameof(Order.AgentId)] = v =>
                {
                    if (long.TryParse(v, out var agentId))
                        query = query.Where(x => x.AgentId == agentId);
                },
                [nameof(Order.Amount)] = v =>
                {
                    var val = decimal.Parse(v.Replace(",", "."), new NumberFormatInfo() { NumberDecimalSeparator = "." });
                    query = query.Where(x => x.Amount == val);
                },
                [nameof(Order.Comment)] = v =>
                {
                    // Для %value% обязательно pg_trgm индекс
                    query = query.Where(x => x.Comment != null && EF.Functions.ILike(x.Comment, $"%{v}%"));
                },
                [nameof(Order.Type)] = v =>
                {
                    if (Enum.TryParse(v, out OrderType type))
                        query = query.Where(x => x.Type == type);
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
}
