using static Warehouse.Web.Managers.Manager;

namespace Warehouse.Web.Managers;

internal class ManagerDeletedEvent : DomainEventBase
{
    public ManagerDeletedEvent(ManagerSnapshot manager, Guid userId, long storeId)
    {
        Manager = manager;
        UserId = userId;
        StoreId = storeId;
    }

    public ManagerSnapshot Manager { get; }
    public Guid UserId { get; }
    public long StoreId { get; }
}
