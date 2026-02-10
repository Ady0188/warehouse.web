namespace Warehouse.Web.Operations.Endpoints;

public record OperationProductRequest(long ProductId, int Code, string Name, string Unit, string Manufacturer, int Quantity, decimal Price, decimal BuyPrice, decimal SellPrice, int Difference);
