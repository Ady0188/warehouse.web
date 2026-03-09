using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using Warehouse.Web.Operations.Endpoints;

namespace Warehouse.Web.Operations
{
    internal static class Extensions
    {
        public static string GetTitle(this int input) => input switch
        {
            0 => "Отправка перемещение",
            1 => "Прием перемещение",
            2 => "Приемка товара",
            3 => "Возврат поставщику",
            4 => "Отгрузка",
            5 => "Возврат покупателю",
            6 => "Ревизия склада",
            7 => "Перемещение"
        };

        public static string ToJson(this object o) => System.Text.Json.JsonSerializer.Serialize(o);
        public static GetAllOptions ToOptions(this PagedRequest request, long storeId) => new GetAllOptions
        {
            StoreId = storeId,
            Page = request.Page,
            PageSize = request.PageSize,
            SortField = !string.IsNullOrEmpty(request.SortField) ? request.SortField.Trim('+', '-') : request.SortField,
            SortOrder = string.IsNullOrEmpty(request.SortField) ? SortOrder.Unsorted :
                                request.SortField.EndsWith('+') ? SortOrder.Ascending : SortOrder.Descending,
            Filter = string.IsNullOrEmpty(request.Search) ? request.Filter : request.Search
        };

        public static IQueryable<Operation> ApplySorting(this IQueryable<Operation> query, GetAllOptions p)
        {
            if (string.IsNullOrWhiteSpace(p.SortField))
                return query.OrderByDescending(x => x.Date); // сортировка по умолчанию

            var sortMap = new Dictionary<string, Expression<Func<Operation, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(Operation.Id)] = x => x.Id,
                [nameof(Operation.Code)] = x => x.Code,
                [nameof(Operation.Date)] = x => x.Date,
                [nameof(Operation.Amount)] = x => x.Amount,
                [nameof(Operation.Discount)] = x => x.Discount,
                [nameof(Operation.Type)] = x => x.Type,
                [nameof(Operation.Comment)] = x => x.Comment,
                [nameof(Operation.StoreId)] = x => x.StoreId,
                [nameof(Operation.AgentId)] = x => x.AgentId
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
        public static IQueryable<Operation> ApplyFilters(this IQueryable<Operation> query, GetAllOptions p)
        {
            if (string.IsNullOrWhiteSpace(p.Filter))
                return query;

            var handlers = new Dictionary<string, Action<string>>(
                StringComparer.OrdinalIgnoreCase)
            {
                [nameof(Operation.CreateDate)] = v =>
                {
                    if (TryParseDate(v, out var startUtc))
                    {
                        var endUtc = startUtc.AddDays(1);
                        query = query.Where(x => x.CreateDate >= startUtc && x.CreateDate < endUtc);
                    }
                },
                [nameof(Operation.UpdateDate)] = v =>
                {
                    if (TryParseDate(v, out var startUtc))
                    {
                        var endUtc = startUtc.AddDays(1);
                        query = query.Where(x => x.UpdateDate >= startUtc && x.UpdateDate < endUtc);
                    }
                },
                [nameof(Operation.Date)] = v =>
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
                [nameof(Operation.Code)] = v =>
                {
                    if (int.TryParse(v, out var code))
                        query = query.Where(x => x.Code == code);
                },
                [nameof(Operation.Id)] = v =>
                {
                    if (long.TryParse(v, out var id))
                        query = query.Where(x => x.Id == id);
                },
                [nameof(Operation.StoreId)] = v =>
                {
                    if (long.TryParse(v, out var storeId))
                        query = query.Where(x => x.StoreId == storeId);
                },
                [nameof(Operation.ToStoreId)] = v =>
                {
                    if (long.TryParse(v, out var storeId))
                        query = query.Where(x => x.ToStoreId == storeId);
                },
                [nameof(Operation.AgentId)] = v =>
                {
                    if (long.TryParse(v, out var agentId))
                        query = query.Where(x => x.AgentId == agentId);
                },
                [nameof(Operation.Amount)] = v =>
                {
                    if (decimal.TryParse(v.Replace(",", "."), NumberStyles.Number, new NumberFormatInfo { NumberDecimalSeparator = "." }, out var val))
                        query = query.Where(x => x.Amount == val);
                },
                [nameof(Operation.Discount)] = v =>
                {
                    if (decimal.TryParse(v.Replace(",", "."), NumberStyles.Number, new NumberFormatInfo { NumberDecimalSeparator = "." }, out var val))
                        query = query.Where(x => x.Discount == val);
                },
                [nameof(Operation.Comment)] = v =>
                {
                    // Для %value% обязательно pg_trgm индекс
                    query = query.Where(x => x.Comment != null && EF.Functions.ILike(x.Comment, $"%{v}%"));
                },
                [nameof(Operation.Type)] = v =>
                {
                    if (Enum.TryParse(v, out OperationType type))
                        query = query.Where(x => x.Type == type);
                }
            };

            var filterData = p.Filter;
            bool isReceive = false;

            foreach (var item in filterData.Split(")and("))
            {
                var fieldValue = item.Trim('(', ')').Split(',');
                if (fieldValue.Length < 2) continue;

                var field = fieldValue[0]?.Trim();
                var value = Uri.UnescapeDataString(fieldValue[1]?.Trim() ?? string.Empty).ToLower();

                if (field == "Type")
                    isReceive = value == "1";

                if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value))
                    continue;

                if (handlers.TryGetValue(field, out var apply))
                    apply(value);
            }

            //if (p.ToStoreId > 0 && isReceive && handlers.TryGetValue("ToStoreId", out var _apply1))
            //    _apply1(p.ToStoreId.ToString());
            //if (p.StoreId > 0 && handlers.TryGetValue("StoreId", out var _apply2))
            //    _apply2(p.StoreId.ToString());
            if (isReceive && handlers.TryGetValue("ToStoreId", out var _apply1))
                _apply1(p.StoreId.ToString());
            else if (p.StoreId > 0 && handlers.TryGetValue("StoreId", out var _apply2))
                _apply2(p.StoreId.ToString());

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
