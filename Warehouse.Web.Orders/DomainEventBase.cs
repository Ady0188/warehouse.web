using MediatR;

namespace Warehouse.Web.Orders
{
    public abstract class DomainEventBase : INotification
    {
        public DateTime DateOccurred { get; protected set; } = DateTime.Now;
    }
}
