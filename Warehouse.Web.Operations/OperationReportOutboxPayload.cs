using Warehouse.Web.Contracts;

namespace Warehouse.Web.Operations;

internal class OperationReportOutboxPayload
{
    public OperationReportDto Operation { get; set; } = new();
    public OperationMethod Method { get; set; }

    public static OperationReportOutboxPayload FromEvent(OperationReportEvent notification)
    {
        int inOrOut =
            (notification.Operation.Type == OperationType.Send
             || notification.Operation.Type == OperationType.ReturnsToSuplier
             || notification.Operation.Type == OperationType.Shipments)
            ? -1
            : 1;

        var dto = new OperationReportDto
        {
            StoreId = notification.Operation.Type == OperationType.Receive ? notification.Operation.ToStoreId : notification.Operation.StoreId,
            StoreName = notification.StoreName,
            ManagerId = notification.ManagerId,
            ManagerName = notification.ManagerName,
            ManagerPhone = notification.ManagerPhone,
            AgentId = notification.Operation.AgentId,
            AgentName = notification.Operation.Type == OperationType.Send || notification.Operation.Type == OperationType.Receive ? notification.ToStoreName : notification.AgentName,
            AgentPhone = notification.AgentPhone,
            AgentAddress = notification.AgentAddress,
            ObjectId = notification.Operation.Id,
            ObjectParentId = notification.Operation.ParentId,
            ObjectCode = notification.Operation.Code,
            ObjectName = nameof(Operation),
            ObjectType = (short)notification.Operation.Type,
            IsReceived = notification.Operation.IsReceived,
            Amount = notification.Operation.Amount * inOrOut,
            DisctountPercentage = notification.Operation.Discount,
            Date = notification.Operation.Date,
            Products = notification.Operation.Products.Select(p => new OperationProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.Name,
                Quantity = p.Quantity * inOrOut,
                Price = p.Price,
                BuyPrice = p.BuyPrice,
                SellPrice = p.SellPrice,
                Manufacturer = p.Manufacturer,
                Unit = p.Unit,
                ProductCode = p.Code,
                Difference = p.Difference
            }).ToList()
        };

        return new OperationReportOutboxPayload
        {
            Operation = dto,
            Method = notification.Method
        };
    }
}
