namespace Warehouse.Web.Shared.Requests;

public class OrderAgentRequest
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal Debt { get; set; }
    public decimal Difference { get; set; }
}

public record CreateOrderRequest(string Date, long? DocId, long StoreId, long AgentId, decimal Amount, string Comment, int Type, List<OrderAgentRequest>? Agents);
public record UpdateOrderRequest(string Date, long? DocId, long StoreId, long AgentId, decimal Amount, string Comment, int Type, List<OrderAgentRequest>? Agents);