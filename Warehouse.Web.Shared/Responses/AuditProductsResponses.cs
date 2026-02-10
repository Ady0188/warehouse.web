namespace Warehouse.Web.Shared.Responses;

public class AuditProductResponse
{
    public long Id { get; set; }
    public int Code { get; set; }
    public DateTime Date { get; set; }
    public long ShortageCount => Products.Where(x => x.Difference < 0).Sum(x => Math.Abs(x.Difference));
    public decimal ShortageAmount => Products.Where(x => x.Difference < 0).Sum(x => Math.Abs(x.Difference) * x.SellPrice);
    public long SurplusCount => Products.Where(x => x.Difference > 0).Sum(x => x.Difference);
    public decimal SurplusAmount => Products.Where(x => x.Difference > 0).Sum(x => x.Difference * x.SellPrice);
    public string Comment { get; set; }
    public string StoreName { get; set; }
    public long StoreId { get; set; }
    public List<OperationProductResponse> Products { get; set; } = new List<OperationProductResponse>();
}

public class AuditProductsResponse
{
    public int Total { get; set; }
    public List<StoreResponse> Stores { get; set; } = new List<StoreResponse>();
    public List<ProductResponse> Products { get; set; } = new List<ProductResponse>();
    public List<AuditProductResponse> Items { get; set; } = new List<AuditProductResponse>();
}
