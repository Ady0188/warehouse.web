using MediatR;
using Microsoft.Extensions.Logging;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Reporting.Integrations;

internal class NewOperationIntegrationHandler : INotificationHandler<OperationIntegrationEvent>
{
    private readonly ILogger<NewOperationIntegrationHandler> _logger;
    private readonly ProductTurnoverIngestionService _productTurnoverIngestionService;
    private readonly AgentRemainsIngestionService _agentRemainsIngestionService;
    private readonly IMediator _mediator;

    public NewOperationIntegrationHandler(ILogger<NewOperationIntegrationHandler> logger, ProductTurnoverIngestionService productTurnoverIngestionService, AgentRemainsIngestionService agentRemainsIngestionService, IMediator mediator)
    {
        _logger = logger;
        _productTurnoverIngestionService = productTurnoverIngestionService;
        _agentRemainsIngestionService = agentRemainsIngestionService;
        _mediator = mediator;
    }
    
    public async Task Handle(OperationIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Handling operation event (Method={Method}, OperationId={OperationId})",
            notification.Method, notification.Operation?.ObjectId);

        var operation = notification.Operation;
        if (operation == null)
        {
            _logger.LogWarning("Notification contains no operation. Nothing to do.");
            return;
        }

        try
        {
            // Подготовка DTO
            var turnover = new ProductTurnover
            {
                StoreId = operation.StoreId,
                StoreName = operation.StoreName,
                //ToStoreId = operation.ToStoreId,
                //ToStoreName = operation.ToStoreName,
                ManagerId = operation.ManagerId,
                ManagerName = operation.ManagerName,
                ManagerPhone = operation.ManagerPhone,
                AgentId = operation.AgentId,
                AgentName = operation.AgentName,
                AgentPhone = operation.AgentPhone,
                AgentAddress = operation.AgentAddress,
                ObjectId = operation.ObjectId,
                ObjectParentId = operation.ObjectParentId,
                ObjectCode = operation.ObjectCode,
                ObjectName = operation.ObjectName,
                ObjectType = operation.ObjectType,
                IsReceived = operation.IsReceived,
                Amount = operation.Amount,
                Discount = operation.Discount,
                Date = operation.Date,
                Products = operation.Products?.Select(p => new Product
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Manufacturer = p.Manufacturer,
                    Unit = p.Unit,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    BuyPrice = p.BuyPrice,
                    SellPrice = p.SellPrice,
                    Difference = p.Difference
                }).ToList() ?? new List<Product>()
            };

            var remains = new AgentRemains
            {
                StoreId = operation.StoreId,
                StoreName = operation.StoreName,
                ManagerId = operation.ManagerId,
                ManagerName = operation.ManagerName,
                AgentId = operation.AgentId,
                AgentName = operation.AgentName,
                ObjectId = operation.ObjectId,
                ObjectCode = operation.ObjectCode,
                ObjectName = operation.ObjectName,
                ObjectType = operation.ObjectType,
                Amount = operation.Amount,
                Discount = operation.Discount,
                Date = operation.Date
            };

            switch (notification.Method)
            {
                case OperationMethod.Create:
                    {
                        _logger.LogDebug("Creating reporting records for object {ObjectId}:{ObjectName}", operation.ObjectId, operation.ObjectName);
                        
                        // Выполняем добавление turnover и агентских отчётов.
                        // Эти два действия пишут в разные таблицы — можно запускать параллельно.
                        var productTask = _productTurnoverIngestionService.AddAsync(turnover);
                        Task agentTask = Task.CompletedTask;

                        if (operation.AgentId != 0)
                        {
                            agentTask = _agentRemainsIngestionService.AddReportAsync(remains);
                        }

                        // Ждём оба таска; если один упадёт — выйдет исключение и попадёт в catch ниже
                        await Task.WhenAll(productTask, agentTask);

                        _logger.LogInformation("Create handling finished for object {ObjectId}:{ObjectName}", operation.ObjectId, operation.ObjectName);
                        break;
                    }
                case OperationMethod.Update:
                    {
                        _logger.LogDebug("Updating reporting records for object {ObjectId}:{ObjectName}", operation.ObjectId, operation.ObjectName);

                        var updatedTurnover = await _productTurnoverIngestionService.UpdateByObjectAsync(turnover);
                        if (!updatedTurnover)
                        {
                            _logger.LogWarning("Turnover not found for update (object_id={ObjectId}, object_name={ObjectName}). Consider Upsert if needed.",
                                operation.ObjectId, operation.ObjectName);
                            // опционально: вызвать AddAsync (upsert behavior) или сигнализировать об особой логике
                        }
                        else
                        {
                            _logger.LogInformation("Turnover updated for object {ObjectId}:{ObjectName}", operation.ObjectId, operation.ObjectName);
                        }

                        if (operation.AgentId != 0)
                        {
                            var updatedAgent = await _agentRemainsIngestionService.UpdateReportAsync(remains);
                            if (!updatedAgent)
                            {
                                _logger.LogWarning("Agent remains not found for update (object_id={ObjectId}, object_name={ObjectName}).", operation.ObjectId, operation.ObjectName);
                                // опционально: AddReportAsync(remains);
                            }
                            else
                            {
                                _logger.LogInformation("Agent remains updated for object {ObjectId}:{ObjectName}", operation.ObjectId, operation.ObjectName);
                            }
                        }

                        break;
                    }
                case OperationMethod.Delete:
                    {
                        _logger.LogDebug("Deleting reporting records for object {ObjectId}:{ObjectName}", operation.ObjectId, operation.ObjectName);

                        // Удаляем по (object_id, object_type)
                        var deletedTurnovers = await _productTurnoverIngestionService.DeleteByObjectAsync(operation.ObjectId, operation.ObjectType);
                        _logger.LogInformation("Deleted {Count} product_turnovers for object {ObjectId}:{ObjectName}", deletedTurnovers, operation.ObjectId, operation.ObjectName);

                        if (operation.AgentId != 0)
                        {
                            var deletedRemains = await _agentRemainsIngestionService.DeleteReportAsync(operation.ObjectId, operation.ObjectType);
                            _logger.LogInformation("Deleted {Count} agent_remains for object {ObjectId}:{ObjectName}", deletedRemains, operation.ObjectId, operation.ObjectName);
                        }

                        break;
                    }
                default:
                    _logger.LogWarning("Unknown OperationMethod: {Method}", notification.Method);
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Handling operation was cancelled (ObjectId={ObjectId})", operation.ObjectId);
            throw; // пробрасываем, чтобы upstream мог корректно обработать отмену
        }
        catch (Exception ex)
        {
            // Логируем подробную информацию и пробрасываем дальше, чтобы механизм доставки событий (или retry policy) смог среагировать.
            _logger.LogError(ex, "Error while handling operation (ObjectId={ObjectId}, Method={Method})", operation.ObjectId, notification.Method);
            throw;
        }
    }
}
