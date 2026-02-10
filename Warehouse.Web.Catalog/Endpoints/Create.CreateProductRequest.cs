namespace Warehouse.Web.Catalog.Endpoints;

public record CreateProductRequest(string Name, string? Manufacturer, string Unit, decimal BuyPrice, decimal SellPrice, int limit);
