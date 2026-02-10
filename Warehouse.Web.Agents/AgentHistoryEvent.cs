using Warehouse.Web.Contracts;
using static Warehouse.Web.Agents.Agent;

namespace Warehouse.Web.Agents;

internal class AgentHistoryEvent : DomainEventBase
{
    public AgentHistoryEvent(Agent newAgent, AgentSnapshot? oldAgent, HistoryMethod method, string? userName, string? storeName, string? objectStoreName, string? objectManagerName)
    {
        OldAgent = oldAgent;
        NewAgent = newAgent;
        UserName = userName;
        StoreName = storeName;
        Method = method;
        ObjectStoreName = objectStoreName ?? string.Empty;
        ObjectManagerName = objectManagerName ?? string.Empty;
    }

    public AgentSnapshot? OldAgent { get; }
    public Agent NewAgent { get; }
    public string? UserName { get; }
    public string? StoreName { get; }
    public string ObjectStoreName { get; }
    public string ObjectManagerName { get; }
    public HistoryMethod Method { get; }
}
