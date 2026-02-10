using Warehouse.Web.Contracts;
using static Warehouse.Web.Managers.Manager;

namespace Warehouse.Web.Managers;

internal class ManagerHistoryEvent : DomainEventBase
{
    public ManagerHistoryEvent(Manager newManager, ManagerSnapshot? oldManager, HistoryMethod method, string? userName, string? storeName, string? objectStoreName)
    {
        OldManager = oldManager;
        NewManager = newManager;
        UserName = userName;
        StoreName = storeName;
        Method = method;
        ObjectStoreName = objectStoreName ?? string.Empty;
    }

    public ManagerSnapshot? OldManager { get; }
    public Manager NewManager { get; }
    public string? UserName { get; }
    public string? StoreName { get; }
    public string ObjectStoreName { get; }
    public HistoryMethod Method { get; }
}
