namespace Warehouse.Web.Shared.Requests;
public class OperationProductRequest
{
    public long ProductId { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public string? Manufacturer { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string PriceStr => Price.ToString();
    public decimal BuyPrice { get; set; }
    public string BuyPriceStr => BuyPrice.ToString();
    public decimal SellPrice { get; set; }
    public string SellPriceStr => SellPrice.ToString();

    //For audit
    public int Difference { get; set; }
}

public record CreateOperationRequest(string Date, decimal Amount, decimal Discount, int Type, long StoreId, long ReceiverId, string? Comment, List<OperationProductRequest> Products);
public record UpdateOperationRequest(string Date, decimal Amount, decimal Discount, int Type, long ParentId, long StoreId, long ReceiverId, string? Comment, List<OperationProductRequest> Products);