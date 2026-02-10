using MediatR;
using Microsoft.Extensions.Logging;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Reporting.Integrations;
internal class NewHistoryCreatedIntegrationHandler : INotificationHandler<HistoryCreatedIntegrationEvent>
{
    private readonly ILogger<NewHistoryCreatedIntegrationHandler> _logger;
    private readonly HistoryIngestionService _historyIngestionService;
    private readonly IMediator _mediator;

    public NewHistoryCreatedIntegrationHandler(ILogger<NewHistoryCreatedIntegrationHandler> logger, HistoryIngestionService historyIngestionService, IMediator mediator)
    {
        _logger = logger;
        _historyIngestionService = historyIngestionService;
        _mediator = mediator;
    }

    public async Task Handle(HistoryCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling history created event to populate reporting database...");

            var history = notification.History;

            await _historyIngestionService.AddHistoryAsync(new History
            {
                StoreName = history.StoreName ?? string.Empty,
                UserName = history.UserName ?? string.Empty,
                Method = history.Method,
                ObjectName = history.ObjectName,
                OldData = history.OldData,
                NewData = history.NewData,
                ObjectId = history.ObjectId,
                ObjectStoreName = history.ObjectStoreName,
                ObjectManagerName = history.ObjectManagerName,
                ObjectAgentName = history.ObjectAgentName
            });
        }
        catch (Exception ex)
        {

            throw;
        }
    }
}