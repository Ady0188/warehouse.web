namespace Warehouse.Web.Shared.Responses;

public class AgentTurnoverResponse
{
    public long StoreId { get; set; }
    public string StoreName { get; set; }
    public long AgentId { get; set; }
    public string AgentName { get; set; }
    public int Code { get; set; }
    public string Operation { get; set; }
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public DateTime Date { get; set; }
}
public class AgentTurnoversResponse
{
    public int Total { get; set; }
    public decimal StartRemains { get; set; }
    public decimal EndRemains { get; set; }
    public List<AgentTurnoverResponse> Items { get; set; } = new List<AgentTurnoverResponse>();
}
public class TurnoverResponse
{
    public long StoreId { get; set; }
    public string StoreName { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; }
    public long AgentId { get; set; }
    public string AgentName { get; set; }
    public string Operation { get; set; }
    public decimal Price { get; set; }
    public long Quantity { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}
public class TurnoversResponse
{
    public int Total { get; set; }
    public long StartRemains { get; set; }
    public long EndRemains { get; set; }
    public List<TurnoverResponse> Items { get; set; } = new List<TurnoverResponse>();
}

public class RemainsResponse
{
    public long ProductId { get; set; }
    public long Count { get; set; }
}
public class RemainsesResponse
{
    public Dictionary<long, List<RemainsResponse>> StoreRemains { get; set; }
}