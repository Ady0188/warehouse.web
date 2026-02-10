using MediatR;

namespace Warehouse.Web.Contracts;
public class AgentRemainsIntegrationEvent : INotification
{
    public AgentRemainsDto Remains { get; private set; }
    public AgentRemainsIntegrationEvent(AgentRemainsDto remains)
    {
        Remains = remains;
    }
}
