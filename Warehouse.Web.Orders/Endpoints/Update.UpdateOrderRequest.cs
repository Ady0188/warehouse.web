using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Web.Orders.Endpoints
{
    public record UpdateOrderRequest([FromRoute] long Id, string Date, long? DocId, long StoreId, long AgentId, decimal Amount, string? Comment, int Type);
}
