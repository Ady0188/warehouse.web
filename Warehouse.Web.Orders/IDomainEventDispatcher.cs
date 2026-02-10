namespace Warehouse.Web.Orders
{
    internal interface IDomainEventDispatcher
    {
        Task DispatchAndClearEvents(IEnumerable<IHaveDomainEvents> entitiesWithEvents);
    }
}
