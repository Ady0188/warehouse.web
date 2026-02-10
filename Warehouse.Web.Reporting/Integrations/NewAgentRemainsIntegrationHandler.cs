using MediatR;
using Microsoft.Extensions.Logging;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Reporting.Integrations;
internal class NewAgentRemainsIntegrationHandler : INotificationHandler<AgentRemainsIntegrationEvent>
{
    private readonly ILogger<NewAgentRemainsIntegrationHandler> _logger;
    private readonly AgentRemainsIngestionService _agentRemainsIngestionService;
    private readonly IMediator _mediator;

    public NewAgentRemainsIntegrationHandler(ILogger<NewAgentRemainsIntegrationHandler> logger, AgentRemainsIngestionService agentRemainsIngestionService, IMediator mediator)
    {
        _logger = logger;
        _agentRemainsIngestionService = agentRemainsIngestionService;
        _mediator = mediator;
    }
    public async Task Handle(AgentRemainsIntegrationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling agent remains created event to populate reporting database...");

            var remains = notification.Remains;

            await _agentRemainsIngestionService.AddReportAsync(new AgentRemains
            {
                StoreId = remains.StoreId,
                StoreName = remains.StoreName,
                ManagerId = remains.ManagerId,
                ManagerName = remains.ManagerName,
                AgentId = remains.AgentId,
                AgentName = remains.AgentName,
                ObjectId = remains.ObjectId,
                ObjectCode = remains.ObjectCode,
                ObjectName = remains.ObjectName,
                ObjectType = remains.ObjectType,
                Amount = remains.Amount,
                Discount = remains.Disctount,
                Date = remains.Date
            });
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
