namespace Warehouse.Web.Shared.Responses;
public class ManagerResponse
{
    public long Id { get; init; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public long StoreId { get; set; }
    public string StoreName { get; set; }
}

public class ManagersResponse
{
    public List<StoreResponse> Stores { get; set; } = new List<StoreResponse>();
    public List<ManagerResponse> Items { get; set; } = new List<ManagerResponse>();
    public int Total { get; set; }
}
