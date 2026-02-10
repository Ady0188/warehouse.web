namespace Warehouse.Web.Reporting.Integrations;
internal class AgentRemains
{
    public Guid Id { get; set; }
    public long StoreId { get; set; }
    public string StoreName { get; set; }
    public long ManagerId { get; set; }
    public string ManagerName { get; set; }
    public long AgentId { get; set; }
    public string AgentName { get; set; }
    public long ObjectId { get; set; }
    public int ObjectCode { get; set; }
    public string ObjectName { get; set; }
    public int ObjectType { get; set; }
    public decimal Amount { get; set; }
    public decimal Discount { get; set; }
    public DateTime Date { get; set; }
}
