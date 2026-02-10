using Warehouse.Web.Contracts;

namespace Warehouse.Web.Operations;

internal class OperationReportEvent : DomainEventBase
{
    public OperationReportEvent(Operation operation, string storeName, string toStoreName, string agentName, string agentPhone, string agentAddress, long managerId, string managerName, string managerPhone, OperationMethod method)
    {
        Operation = operation;
        StoreName = storeName;
        ToStoreName = toStoreName;
        AgentName = agentName;
        AgentPhone = agentPhone;
        AgentAddress = agentAddress;
        ManagerId = managerId;
        ManagerName = managerName;
        ManagerPhone = managerPhone;
        Method = method;
    }
    public Operation Operation { get; }
    public string StoreName { get; }
    public string ToStoreName { get; }
    public string AgentName { get; }
    public string AgentPhone { get; }
    public string AgentAddress { get; }
    public long ManagerId { get; }
    public string ManagerName { get; }
    public string ManagerPhone { get; }
    public OperationMethod Method { get; }
}
