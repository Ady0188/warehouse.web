namespace Warehouse.Web.Client.Models;

public class AppSettings
{
    public MenuType MenuType { get; set; } = MenuType.Side;
    public bool IsDarkTheme { get; set; } = false;
    public int ThemeIndex { get; set; }
}