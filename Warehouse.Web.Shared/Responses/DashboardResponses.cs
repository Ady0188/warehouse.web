namespace Warehouse.Web.Shared.Responses;

public class StoreMonthTradeTurnoversResponse
{
    public long StoreId { get; init; }
    public List<StoreDateTradeTurnoverResponse> DateTrade { get; set; } = new List<StoreDateTradeTurnoverResponse>();
    public List<StoreMonthTradeTurnoverResponse> MonthTrade { get; init; } = new List<StoreMonthTradeTurnoverResponse>();
}

public class StoreDateTradeTurnoverResponse
{
    public long StoreId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

public class StoreMonthTradeTurnoverResponse : StoreDateTradeTurnoverResponse
{
}
