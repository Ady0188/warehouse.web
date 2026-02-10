using MediatR;

namespace Warehouse.Web.Contracts;

public enum OperationMethod
{
    Create,
    Update,
    Delete
}

public class OperationIntegrationEvent : INotification
{
    public OperationReportDto Operation { get; private set; }
    public OperationMethod Method { get; private set; }

    public OperationIntegrationEvent(OperationReportDto operation, OperationMethod method)
    {
        Operation = operation;
        Method = method;
    }
}
