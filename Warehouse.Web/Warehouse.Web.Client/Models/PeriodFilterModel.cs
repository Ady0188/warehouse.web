namespace Warehouse.Web.Client.Models;

public sealed class PeriodFilterModel
{
    public PeriodType Type { get; set; }

    // Для Month
    public int? Year { get; set; }
    public int? Month { get; set; }

    // Для Range
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}