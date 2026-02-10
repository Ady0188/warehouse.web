namespace Warehouse.Web.Contracts;

public class OperationReportDto
{
    public long StoreId { get; set; }
    public string StoreName { get; set; }
    //public long ToStoreId { get; set; }
    //public string ToStoreName { get; set; }
    public long ManagerId { get; set; }
    public string ManagerName { get; set; }
    public string ManagerPhone { get; set; }
    public long AgentId { get; set; }
    public string AgentName { get; set; }
    public string AgentPhone { get; set; }
    public string AgentAddress { get; set; }
    public long ObjectId { get; set; }
    public long ObjectParentId { get; set; }
    public int ObjectCode { get; set; }
    public string ObjectName { get; set; }
    public int ObjectType { get; set; }
    public bool IsReceived { get; set; }
    public decimal Amount { get; set; }
    public decimal DisctountPercentage { get; set; }
    public decimal Discount => Amount * DisctountPercentage / 100;
    public decimal Total => Amount - Discount;
    public DateTime Date { get; set; }

    public List<OperationProductDto> Products { get; set; }
}
