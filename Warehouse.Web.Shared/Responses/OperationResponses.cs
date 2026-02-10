using System.Text.Json.Serialization;

namespace Warehouse.Web.Shared.Responses;

public class OperationProductResponse
{
    public long Id { get; set; }
    public int Code { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; }
    public string Unit { get; set; }
    public string? Manufacturer { get; set; }

    //For audit
    public long Plan { get; set; }
    public long Fact { get; set; }
    public long Difference { get; set; }
    public bool Changed { get; set; }
}

public class OperationResponse
{
    public long Id { get; set; }
    public int Code { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal Discount => Amount * DiscountPercentage / 100;
    public decimal ToPay => Amount - Discount;
    public int Type { get; set; }
    public string Comment { get; set; }
    public string StoreName { get; set; }
    public string ToStoreName { get; set; }
    public long StoreId { get; set; }
    public long ToStoreId { get; set; }
    public long AgentId { get; set; }
    public AgentResponse? Agent { get; set; }
    public string? AgentName { get; set; }
    public string? ManagerName { get; set; }
    public long UserId { get; set; }
    public bool IsReceived { get; set; }
    public List<OperationProductResponse> Products { get; set; } = new List<OperationProductResponse>();
}
public class OperationsResponse
{
    public int Total { get; set; }
    public long TotalProductCount { get; set; }
    public decimal TotalProductAmount { get; set; }
    public decimal TotalProductDiscount { get; set; }
    public decimal TotalProductToPay { get; set; }
    public List<StoreResponse> Stores { get; set; } = new List<StoreResponse>();
    public List<StoreResponse> ToStores { get; set; } = new List<StoreResponse>();
    public List<AgentResponse> Agents { get; set; } = new List<AgentResponse>();
    public List<ProductResponse> Products { get; set; } = new List<ProductResponse>();
    public List<OperationResponse> NotResivedItems { get; set; } = new List<OperationResponse>();
    public List<OperationResponse> Items { get; set; } = new List<OperationResponse>();
}