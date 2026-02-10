using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Operations.Endpoints;

public record UpdateOperationRequest([FromRoute] long Id, string Date, decimal Amount, decimal Discount, int Type, long ParentId, long StoreId, long ReceiverId, string? Comment, List<OperationProductRequest> Products);
