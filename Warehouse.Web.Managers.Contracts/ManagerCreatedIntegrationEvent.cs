using MediatR;

namespace Warehouse.Web.Managers.Contracts;

public class ManagerCreatedIntegrationEvent : INotification
{
    public DateTime CreatedDate { get; private set; } = DateTime.Now;
    public ManagerDetailsDto Manager { get; private set; }
    public ManagerCreatedIntegrationEvent(ManagerDetailsDto manager)
    {
        Manager = manager;
    }
}
