namespace Warehouse.Web.Managers.Contracts;

public class ManagerDetailsDto
{
    public long Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public DateTime? DeleteDate { get; set; }
    public long StoreId { get; set; }
}
