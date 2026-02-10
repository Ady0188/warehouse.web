using MudBlazor;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Warehouse.Web.Client.Models;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Client.Helpers;
public static class Extensions
{
    public static decimal AppParce(this string input) => decimal.Parse(input.Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." });
    
    public static Dictionary<long, long> Clone(this Dictionary<long, long> storesRemains) => storesRemains.ToDictionary(x => x.Key, x => x.Value);
    public static List<ProductResponse> Clone(this List<ProductResponse> list) => list.Select(l => new ProductResponse
    {
        Name = l.Name,
        BuyPrice = l.BuyPrice,
        Code = l.Code,
        CreateDate = l.CreateDate,
        Id = l.Id,
        LimitRemain = l.LimitRemain,
        Manufacturer = l.Manufacturer,
        SellPrice = l.SellPrice,
        StoresRemains = l.StoresRemains.Clone(),
        Unit = l.Unit,
        UpdateDate = l.UpdateDate
    }).ToList();

    public static List<AgentResponse> Clone(this List<AgentResponse> list) => list.Select(l => new AgentResponse
    {
        Id = l.Id,
        Name = l.Name,
        Phone = l.Phone,
        Address = l.Address,
        Comment = l.Comment,
        Debts = l.Debts,
        ManagerId = l.ManagerId,
        ManagerName = l.ManagerName,
        ManagerPhone = l.ManagerPhone,
        StoreId = l.StoreId,
        StoreName = l.StoreName
    }).ToList();

    public static List<OperationProductResponse> Clone(this List<OperationProductResponse> list) => list.Select(l => new OperationProductResponse
    {
        BuyPrice = l.BuyPrice,
        Changed = l.Changed,
        Code = l.Code,
        Difference = l.Difference,
        Fact = l.Fact,
        Id = l.Id,
        Manufacturer = l.Manufacturer,
        Plan = l.Plan,
        Price = l.Price,
        ProductId = l.ProductId,
        ProductName = l.ProductName,
        Quantity = l.Quantity,
        SellPrice = l.SellPrice,
        Unit = l.Unit
    }).ToList();

    public static List<OperAgentResponse> Clone(this List<OperAgentResponse> list) => list.Select(l => new OperAgentResponse
    {
        ManagerPhone = l.ManagerPhone,
        StoreId = l.StoreId,
        Address = l.Address,
        Changed = l.Changed,
        Comment = l.Comment,
        Difference = l.Difference,
        Fact = l.Fact,
        Id = l.Id,
        ManagerId = l.ManagerId,
        ManagerName = l.ManagerName,
        Name = l.Name,
        Phone = l.Phone,
        Plan = l.Plan,
        StoreName = l.StoreName
    }).ToList();

    public static Color GetColor(this string input)
    {
        if (input == "Create")
            return Color.Primary;
        else if (input == "Update")
            return Color.Success;
        else if (input == "Delete")
            return Color.Error;

        return Color.Dark;
    }

    public static string GetOperationName(this string input)
    {
        if (input == "Create")
            return "Добавлен";
        else if (input == "Update")
            return "Обновлен";
        else if (input == "Delete")
            return "Удален";

        return "";
    }

    public static void Refresh(this PagedRequest pagedRequest, TableState state)
    {
        pagedRequest.Page = state.Page + 1;
        pagedRequest.PageSize = state.PageSize;
        pagedRequest.SortField = default;

        if (state.SortDirection != SortDirection.None)
            pagedRequest.SortField = $"{state.SortLabel}{(state.SortDirection == SortDirection.Ascending ? Uri.EscapeDataString("+") : Uri.EscapeDataString("-"))}";

    }
    public static string ToQuery(this PagedRequest request)
    {
        return $"page={request.Page}&pagesize={request.PageSize}&search={request.Search}&filter={request.Filter}&sortfield={request.SortField}";
    }

    public static StringContent? ToStringContent(this object obj)
    {
        if (obj == null)
            return default;

        return new StringContent(obj.Serialize(), Encoding.UTF8, "application/json");
    }

    public static string Serialize(this object obj)
    {
        if (obj == null)
            return string.Empty;

        return JsonSerializer.Serialize(obj);
    }

    public static T? Deserialize<T>(this string input)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<T>(input, options);
    }

    public static string ToBas64(this string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);

        return Convert.ToBase64String(bytes);
    }

    public static string FromBas64(this string base64)
    {
        if (string.IsNullOrEmpty(base64))
            return base64;

        return Encoding.UTF8.GetString(Convert.FromBase64String(base64.Replace("\"", "")));
    }
}