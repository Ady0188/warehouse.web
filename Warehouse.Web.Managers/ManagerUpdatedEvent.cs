using static Warehouse.Web.Managers.Manager;

namespace Warehouse.Web.Managers;
internal class ManagerUpdatedEvent : DomainEventBase
{
    public ManagerUpdatedEvent(ManagerSnapshot oldManager, ManagerSnapshot newManager, Guid userId, long storeId)
    {
        OldManager = oldManager;
        NewManager = newManager;
        UserId = userId;
        StoreId = storeId;
    }

    public ManagerSnapshot OldManager { get; }
    public ManagerSnapshot NewManager { get; }
    public Guid UserId { get; }
    public long StoreId { get; }
}