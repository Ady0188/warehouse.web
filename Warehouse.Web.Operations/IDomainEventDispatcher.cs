namespace Warehouse.Web.Operations
{
    internal interface IDomainEventDispatcher
    {
        Task DispatchAndClearEvents(IEnumerable<IHaveDomainEvents> entitiesWithEvents);
    }
}
