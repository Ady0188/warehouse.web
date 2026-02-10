namespace Warehouse.Web.Shared.Responses;

public class ProductResponse
{
    public long Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
    public string? Manufacturer { get; set; }
    public string Unit { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int LimitRemain { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public Dictionary<long, long> StoresRemains { get; set; } = new Dictionary<long, long>();
}
public class ProductsResponse
{
    public int Total { get; set; }
    public Dictionary<long, string> Stores { get; set; } = new Dictionary<long, string>();
    public List<ProductResponse> Items { get; set; } = new List<ProductResponse>();
}


public class ProductResponseOld
{
    public long Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
    public string? Manufacturer { get; set; }
    public string Unit { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int LimitRemain { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public Dictionary<long, long> StoreRemains { get; set; } = new Dictionary<long, long>();
}
public class ProductsResponseOld
{
    public int Total { get; set; }
    public Dictionary<long, string> Stores { get; set; } = new Dictionary<long, string>();
    public List<ProductResponseOld> Items { get; set; } = new List<ProductResponseOld>();
}
