using Warehouse.Web.Shared.Requests;

namespace Warehouse.Web.Orders.Endpoints
{
    public record CreateOrderRequest(string Date, long? DocId, long StoreId, long AgentId, decimal Amount, string? Comment, int Type, List<OrderAgentRequest>? Agents);
}
