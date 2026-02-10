using FastEndpoints;
using MediatR;
using System.Text.Json;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Operations.Integrations;

internal class PublishOperationIntegrationEvent : INotificationHandler<OperationReportEvent>
{
    private readonly IMediator _mediator;

    public PublishOperationIntegrationEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OperationReportEvent notification, CancellationToken cancellationToken)
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
            //ToStoreId = notification.Operation.ToStoreId,
            //ToStoreName = notification.ToStoreName,
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

        var integrationEvent = new OperationIntegrationEvent(dto, notification.Method);

        await _mediator.Publish(integrationEvent);
    }
}
internal class PublishOperationHistoryIntegrationEvent : INotificationHandler<OperationHistoryEvent>
{
    private readonly IMediator _mediator;

    public PublishOperationHistoryIntegrationEvent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OperationHistoryEvent notification, CancellationToken cancellationToken)
    {
        var dto = new HistoryDto
        {
            StoreName = notification.StoreName,
            UserName = notification.UserName,
            Method = notification.Method,
            NewData = JsonSerializer.Serialize(notification.NewOperation),
            ObjectId = notification.NewOperation.Id,
            ObjectName = nameof(Operation),
            ObjectStoreName = notification.ObjectStoreName,
            ObjectAgentName = notification.ObjectAgentName
        };

        if (notification.OldOperation is not null)
            dto.OldData = JsonSerializer.Serialize(notification.OldOperation);

        var integrationEvent = new HistoryCreatedIntegrationEvent(dto);

        await _mediator.Publish(integrationEvent);
    }
}
