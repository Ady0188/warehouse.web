namespace Warehouse.Web.Operations.Endpoints;

public record CreateOperationRequest(string Date, decimal Amount, decimal Discount, int Type, long StoreId, long ReceiverId, string? Comment, List<OperationProductRequest> Products);
