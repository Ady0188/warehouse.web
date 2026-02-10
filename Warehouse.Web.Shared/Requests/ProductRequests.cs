namespace Warehouse.Web.Shared.Requests;

public record CreateProductRequest(string Name, string? Manufacturer, string Unit, decimal BuyPrice, decimal SellPrice, int limit);
public record UpdateProductRequest(string Name, string? Manufacturer, string Unit, decimal BuyPrice, decimal SellPrice, int limit);