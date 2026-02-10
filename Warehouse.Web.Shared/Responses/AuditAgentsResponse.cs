namespace Warehouse.Web.Shared.Responses;

public class AuditAgentResponse
{
    public long Id { get; set; }
    public int Code { get; set; }
    public DateTime Date { get; set; }
    public decimal Shortage => Agents.Where(x => x.Difference < 0).Sum(x => Math.Abs(x.Difference));
    public decimal Surplus => Agents.Where(x => x.Difference > 0).Sum(x => x.Difference);
    public string Comment { get; set; }
    public string StoreName { get; set; }
    public long StoreId { get; set; }
    public List<OperAgentResponse> Agents { get; set; } = new List<OperAgentResponse>();
}

public class AuditAgentsResponse
{
    public int Total { get; set; }
    public List<StoreResponse> Stores { get; set; } = new List<StoreResponse>();
    public List<AgentResponse> Agents { get; set; } = new List<AgentResponse>();
    public List<AuditAgentResponse> Items { get; set; } = new List<AuditAgentResponse>();
}