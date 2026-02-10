namespace Warehouse.Web.Managers;

internal class ManagerCreatedEvent : DomainEventBase
{
    public ManagerCreatedEvent(Manager manager, Guid userId, long storeId)
    {
        Manager = manager;
        UserId = userId;
        StoreId = storeId;
    }

    public Manager Manager { get; }
    public Guid UserId { get; }
    public long StoreId { get; }
}