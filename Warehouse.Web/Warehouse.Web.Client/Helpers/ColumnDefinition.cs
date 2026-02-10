namespace Warehouse.Web.Client.Helpers;

public class ColumnDefinition
{
  public string Title { get; set; } = string.Empty;
  public string PropertyName { get; set; } = string.Empty;
    public bool Sortable { get; set; } = true;
  public string? StringFormat { get; set; }
  public string? Style { get; set; }
  public bool Visible { get; set; } = true;
}
