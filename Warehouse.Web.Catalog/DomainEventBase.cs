using MediatR;

namespace Warehouse.Web.Catalog;

public abstract class DomainEventBase : INotification
{
    public DateTime DateOccurred { get; protected set; } = DateTime.Now;
}
