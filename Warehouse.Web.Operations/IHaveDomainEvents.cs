namespace Warehouse.Web.Operations
{
    internal interface IHaveDomainEvents
    {
        IEnumerable<DomainEventBase> DomainEvents { get; }
        void ClearDomainEvents();
    }
}
