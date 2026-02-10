using MudBlazor;

namespace Warehouse.Web.Client.Helpers;

public interface IAppSnackbarService
{
    void Success(string message = "Данные успешны сохранены");
    void Error(string message = "Ошибка при сохранение");
    void Info(string message);
}
public class AppSnackbarService : IAppSnackbarService
{
    private readonly ISnackbar _snackbar;

    public AppSnackbarService(ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }

    public void Show(string message, Severity severity)
    {
        _snackbar.Add(message, severity);
    }

    public void Success(string message = "Данные успешны сохранены")
    {
        Show(message, Severity.Success);
    }

    public void Error(string message = "Ошибка при сохранение")
    {
        Show(message, Severity.Error);
    }

    public void Info(string message)
    {
        Show(message, Severity.Info);
    }
}
