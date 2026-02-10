using MediatR;

namespace Warehouse.Web.Managers;

public abstract class DomainEventBase : INotification
{
    public DateTime DateOccurred { get; protected set; } = DateTime.Now;
}
