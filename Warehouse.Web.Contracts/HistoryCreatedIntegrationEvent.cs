using MediatR;

namespace Warehouse.Web.Contracts;
public class HistoryCreatedIntegrationEvent : INotification
{
    public HistoryDto History { get; private set; }
    public HistoryCreatedIntegrationEvent(HistoryDto history)
    {
        History = history;
    }
}