using MediatR;

namespace Warehouse.Web.Agents;

public abstract class DomainEventBase : INotification
{
    public DateTime DateOccurred { get; protected set; } = DateTime.Now;
}
