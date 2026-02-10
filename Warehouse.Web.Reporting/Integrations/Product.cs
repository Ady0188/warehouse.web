namespace Warehouse.Web.Reporting.Integrations;

internal class Product
{
    public Guid Id { get; set; }
    public long ProductId { get;set; }
    public int ProductCode { get;set; }
    public string ProductName { get;set; }
    public string Manufacturer { get;set; }
    public string Unit { get;set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }

    //For Audit
    public int Difference { get; set; }
}
