using Warehouse.Web.Contracts;
using static Warehouse.Web.Orders.Order;

namespace Warehouse.Web.Orders
{
    internal class OrderHistoryEvent : DomainEventBase
    {
        public OrderHistoryEvent(Order newOrder, OrderSnapshot? oldOrder, HistoryMethod method, string? userName, string? storeName, string? objectStoreName, string? objectAgentName)
        {
            OldOrder = oldOrder;
            NewOrder = newOrder;
            UserName = userName;
            StoreName = storeName;
            Method = method;
            ObjectStoreName = objectStoreName ?? string.Empty;
            ObjectAgentName = objectAgentName ?? string.Empty;
        }

        public OrderSnapshot? OldOrder { get; }
        public Order NewOrder { get; }
        public string? UserName { get; }
        public string? StoreName { get; }
        public string ObjectStoreName { get; }
        public string ObjectAgentName { get; }
        public HistoryMethod Method { get; }
    }
}
