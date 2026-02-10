using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Linq.Expressions;
using Warehouse.Web.Catalog.Endpoints;

namespace Warehouse.Web.Catalog;

internal static class Extensions
{
    public static string ToJson(this object o) => System.Text.Json.JsonSerializer.Serialize(o);
    public static GetAllOptions ToOptions(this PagedRequest request, int pageSize = 0) => new GetAllOptions
    {
        Page = request.Page,
        PageSize = pageSize == 0 ? request.PageSize : pageSize,
        SortField = !string.IsNullOrEmpty(request.SortField) ? request.SortField.Trim('+', '-') : request.SortField,
        SortOrder = string.IsNullOrEmpty(request.SortField) ? SortOrder.Unsorted :
                            request.SortField.EndsWith('+') ? SortOrder.Ascending : SortOrder.Descending,
        Filter = string.IsNullOrEmpty(request.Search) ? request.Filter : request.Search,
        IncluedeRemains = request.IncluedeRemains ?? false
    };

    public static IQueryable<Product> ApplySorting(this IQueryable<Product> query, GetAllOptions p)
    {
        if (string.IsNullOrWhiteSpace(p.SortField))
            return query.OrderBy(x => x.Name); // сортировка по умолчанию

        var sortMap = new Dictionary<string, Expression<Func<Product, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Product.CreateDate)] = x => x.CreateDate,
            [nameof(Product.UpdateDate)] = x => x.UpdateDate,
            [nameof(Product.Code)] = x => x.Code,
            [nameof(Product.Name)] = x => x.Name,
            ["Manufacturer"] = x => x.Description,
            [nameof(Product.Unit)] = x => x.Unit,
            [nameof(Product.BuyPrice)] = x => x.BuyPrice,
            [nameof(Product.SellPrice)] = x => x.SellPrice,
            [nameof(Product.LimitRemain)] = x => x.LimitRemain
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
    public static IQueryable<Product> ApplyFilters(this IQueryable<Product> query, GetAllOptions p)
    {
        if (string.IsNullOrWhiteSpace(p.Filter))
            return query;

        var handlers = new Dictionary<string, Action<string>>(
            StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Product.CreateDate)] = v =>
            {
                if (TryParseDate(v, out var startUtc))
                {
                    var endUtc = startUtc.AddDays(1);
                    query = query.Where(x => x.CreateDate >= startUtc && x.CreateDate < endUtc);
                }
            },
            [nameof(Product.UpdateDate)] = v =>
            {
                if (TryParseDate(v, out var startUtc))
                {
                    var endUtc = startUtc.AddDays(1);
                    query = query.Where(x => x.UpdateDate >= startUtc && x.UpdateDate < endUtc);
                }
            },
            [nameof(Product.Code)] = v =>
            {
                if (int.TryParse(v, out var code))
                    query = query.Where(x => x.Code == code);
            },
            [nameof(Product.Name)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Name != null && EF.Functions.ILike(x.Name, $"%{v}%"));
            },
            ["Manufacturer"] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Description != null && EF.Functions.ILike(x.Description, $"%{v}%"));
            },
            [nameof(Product.Unit)] = v =>
            {
                // Для %value% обязательно pg_trgm индекс
                query = query.Where(x => x.Unit != null && x.Unit == v);
            },
            [nameof(Product.BuyPrice)] = v =>
            {
                if (decimal.TryParse(v, out var price))
                    query = query.Where(x => x.BuyPrice == price);
            },
            [nameof(Product.SellPrice)] = v =>
            {
                if (decimal.TryParse(v, out var price))
                    query = query.Where(x => x.SellPrice == price);
            },
            [nameof(Product.LimitRemain)] = v =>
            {
                if (int.TryParse(v, out var limit))
                    query = query.Where(x => x.LimitRemain == limit);
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

        // Простой формат: field=value;field=value...
        //foreach (var pair in p.Filter.Split(';', StringSplitOptions.RemoveEmptyEntries))
        //{
        //    var idx = pair.IndexOf('=');
        //    if (idx <= 0) continue;

        //    var rawField = pair[..idx].Trim();
        //    var rawValue = pair[(idx + 1)..].Trim();

        //    if (rawField.Length == 0 || rawValue.Length == 0) continue;

        //    // URL decode и ограничение длины
        //    var value = Uri.UnescapeDataString(rawValue);
        //    if (value.Length > 200) value = value.Substring(0, 200);

        //    if (handlers.TryGetValue(rawField, out var apply))
        //        apply(value);
        //    // неизвестные поля игнорируем
        //}

        return query.AsNoTracking();

        static bool TryParseDate(string value, out DateTime utcStart)
        {
            utcStart = default;
            // предпочтительнее ddMMyyyy
            if (DateTime.TryParseExact(value, new[] { "ddMMyyyy", "ddMMyy" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                // задаём явный UTC-kind
                utcStart = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
                return true;
            }
            return false;
        }
    }
}
