using Warehouse.Web.Contracts;

namespace Warehouse.Web.Orders
{
    internal class AgentRemainsEvent : DomainEventBase
    {
        public AgentRemainsEvent(Order order, string storeName, string agentName, long managerId, string managerName, HistoryMethod method)
        {
            Order = order;
            StoreName = storeName;
            AgentName = agentName;
            ManagerId = managerId;
            ManagerName = managerName;
            Method = method;
        }
        public Order Order { get; }
        public string StoreName { get; }
        public string AgentName { get; }
        public long ManagerId { get; }
        public string ManagerName { get; }
        public HistoryMethod Method { get; }
    }
}
