using Warehouse.Web.Contracts;
using static Warehouse.Web.Operations.Operation;

namespace Warehouse.Web.Operations;
internal class OperationHistoryEvent : DomainEventBase
{
    public OperationHistoryEvent(Operation newOperation, OperationSnapshot? oldOperation, HistoryMethod method, string? userName, string? storeName, string? objectStoreName, string? objectAgentName)
    {
        OldOperation = oldOperation;
        NewOperation = newOperation;
        UserName = userName;
        StoreName = storeName;
        Method = method;
        ObjectStoreName = objectStoreName ?? string.Empty;
        ObjectAgentName = objectAgentName ?? string.Empty;
    }

    public OperationSnapshot? OldOperation { get; }
    public Operation NewOperation { get; }
    public string? UserName { get; }
    public string? StoreName { get; }
    public string ObjectStoreName { get; }
    public string ObjectAgentName { get; }
    public HistoryMethod Method { get; }
}
