namespace Warehouse.Web.Orders
{
    internal interface IHaveDomainEvents
    {
        IEnumerable<DomainEventBase> DomainEvents { get; }
        void ClearDomainEvents();
    }
}
