namespace Warehouse.Web.Shared.Responses;

public class OrderResponse
{
    public long Id { get; set; }
    public int Code { get; set; }
    public DateTime Date { get; set; }
    public long? DocId { get; set; }
    public long StoreId { get; set; }
    public string StoreName { get; set; }
    public string ManagerName { get; set; }
    public long AgentId { get; set; }
    public string AgentName { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public int Type { get; set; }
    public List<OperAgentResponse> Agents { get; set; } = new List<OperAgentResponse>();
}
public class OrdersResponse
{
    public int Total { get; set; }
    public decimal TotalAmount { get; set; }
    public List<StoreResponse> Stores { get; set; } = new List<StoreResponse>();
    public List<AgentResponse> Agents { get; set; } = new List<AgentResponse>();
    public List<OrderResponse> Items { get; set; } = new List<OrderResponse>();
}
