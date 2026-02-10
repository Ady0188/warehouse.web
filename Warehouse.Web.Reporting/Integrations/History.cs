using Warehouse.Web.Contracts;

namespace Warehouse.Web.Reporting.Integrations;

internal class History
{
    public Guid Id { get; set; }
    public string? StoreName { get; set; }
    public string? UserName { get; set; }
    public HistoryMethod Method { get; set; }
    public string? ObjectName { get; set; }
    public string? OldData { get; set; }
    public string NewData { get; set; }
    public long ObjectId { get; set; }
    public string ObjectStoreName { get; set; } = string.Empty;
    public string ObjectManagerName { get; set; } = string.Empty;
    public string ObjectAgentName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}