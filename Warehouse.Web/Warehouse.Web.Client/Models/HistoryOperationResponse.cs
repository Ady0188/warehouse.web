using Warehouse.Web.Shared.Responses;
using static Warehouse.Web.Client.Pages.SaveData;

namespace Warehouse.Web.Client.Models;

public class HistoryProductResponse
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public string Manufacturer { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int Difference { get; set; }
}

public class HistoryOperationResponse
{
    public long Id { get; set; }
    public int Code { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public OperationType Type { get; set; }
    public string? Comment { get; set; }
    public long StoreId { get; set; }
    public long ToStoreId { get; set; }
    public long AgentId { get; set; }
    public long ParentId { get; set; }
    public bool IsReceived { get; set; }
    public List<HistoryProductResponse> Products { get; set; } = new List<HistoryProductResponse>();

    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }

    public OperationResponse Get() => new OperationResponse
    {
        Id = Id,
        Code = Code,
        Date = Date,
        StoreId = StoreId,
        ToStoreId = ToStoreId,
        AgentId = AgentId,
        Amount = Amount,
        Comment = Comment,
        Type = (int)Type,
        IsReceived = IsReceived,
        DiscountPercentage = Discount,
        Products = Products.Select(p => new OperationProductResponse
        {
            Id = p.Id,
            ProductId = p.ProductId,
            ProductName = p.Name,
            Quantity = p.Quantity,
            Price = p.Price,
            BuyPrice = p.BuyPrice,
            SellPrice = p.SellPrice,
            Unit = p.Unit,
            Manufacturer = p.Manufacturer,
            Difference = p.Difference
        }).ToList(),
    };
}