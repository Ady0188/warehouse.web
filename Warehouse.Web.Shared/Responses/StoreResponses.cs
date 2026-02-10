namespace Warehouse.Web.Shared.Responses;
public class StoreResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ManagerResponse> Managers { get; set; } = new List<ManagerResponse>();
}

public class StoresResponse
{
    public List<StoreResponse> Items { get; set; } = new List<StoreResponse>();
    public int Total { get; set; }
}