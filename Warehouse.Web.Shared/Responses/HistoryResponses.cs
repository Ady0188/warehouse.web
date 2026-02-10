namespace Warehouse.Web.Shared.Responses;

public class HistoriesResponse
{
    public List<HistoryResponse> Items { get; set; } = new List<HistoryResponse>();
    public int Total { get; set; }
}
public class HistoryResponse
{
    public Guid Id { get; set; }
    public string StoreName { get; set; }
    public string UserName { get; set; }
    public string Method { get; set; }
    public string ObjectName { get; set; }
    public string? ObjName => ObjectName.GetHistoryObjectName(NewData!);
    public string? OldData { get; set; }
    public string NewData { get; set; }
    public string ObjectStoreName { get; set; }
    public string ObjectManagerName { get; set; }
    public string ObjectAgentName { get; set; }
    public DateTime CreatedDate { get; set; }
}