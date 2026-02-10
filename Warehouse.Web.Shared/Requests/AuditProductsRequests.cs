namespace Warehouse.Web.Shared.Requests;

public class AuditProductRequest
{
    public long ProductId { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public string? Manufacturer { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
}

public record CreateAuditProductRequest(string Date, decimal Amount, decimal Discount, int Type, long StoreId, long AgentId, string? Comment, List<OperationProductRequest> Products);
public record UpdateAuditProductRequest(string Date, decimal Amount, decimal Discount, int Type, long StoreId, long AgentId, string? Comment, List<OperationProductRequest> Products);