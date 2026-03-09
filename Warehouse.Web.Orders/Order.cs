using Ardalis.GuardClauses;
using System.ComponentModel.DataAnnotations.Schema;
using Warehouse.Web.Contracts;
using static Warehouse.Web.Shared.ApiEndpoints.V1;

namespace Warehouse.Web.Orders
{
    internal class Agent
    {
        public long Id { get; private set; }
        public long AgentId { get; private set; }
        public string Name { get; private set; }
        public decimal Debt { get; private set; }

        //For Audit
        public decimal Difference { get; private set; }

        public Agent() {}

        public Agent(long agentId, string name, decimal debt, decimal difference)
        {
            AgentId = agentId;
            Name = name;
            Debt = debt;
            Difference = difference;
        }

        public sealed record AgentSnapshot(long Id, long AgentId, string Name, decimal Debt, decimal Difference);

        public AgentSnapshot ToSnapshot() =>
            new(Id, AgentId, Name, Debt, Difference);

        public void UpdateDebt(decimal debt)
        {
            Debt += debt;
        }
        public void SetDebt(decimal newDebt)
        {
            Debt = newDebt;
        }

        public void UpdateAgent(long agentId, string name, decimal debt, decimal difference)
        {
            AgentId = agentId;
            Name = name;
            Debt = debt;
            Difference = difference;
        }
    }

    internal class Order : IHaveDomainEvents
    {
        public long Id { get; private set; }
        public int Code { get; private set; }
        public DateTime Date { get; private set; }
        public long? DocId { get; private set; }
        public long StoreId { get; private set; }
        public long AgentId { get; private set; }
        public decimal Amount { get; private set; }
        public decimal? AuditCurrentAmount { get; private set; }
        public decimal? AuditFactAmount { get; private set; }
        public string? Comment { get; private set; }
        public OrderType Type { get; private set; }
        public DateTime CreateDate { get; private set; } = DateTime.Now;
        public DateTime UpdateDate { get; private set; } = DateTime.Now;

        private readonly List<Agent> _agents = new();
        public IReadOnlyCollection<Agent> Agents => _agents.AsReadOnly();

        private List<DomainEventBase> _domainEvents = new();
        [NotMapped]
        public IEnumerable<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();
        protected void RegisterDomainEvent(DomainEventBase eventItem) => _domainEvents.Add(eventItem);
        void IHaveDomainEvents.ClearDomainEvents() => _domainEvents.Clear();


        private Order(
            long storeId,
            long agentId,
            OrderType type,
            decimal amount,
            DateTime date,
            long? docId,
            decimal? auditCurrentAmount,
            decimal? auditFactAmount,
            string? comment)
        {
            SetParams(storeId, agentId, type, amount, date, docId, auditCurrentAmount, auditFactAmount, comment);
        }
        private Order(
            long storeId,
            long agentId,
            OrderType type,
            decimal amount,
            DateTime date,
            long? docId,
            string? comment)
        {
            SetParams(storeId, agentId, type, amount, date, docId, default, default, comment);
        }

        private void SetParams(long storeId, long agentId, OrderType type, decimal amount, DateTime date, long? docId, decimal? auditCurrentAmount, decimal? auditFactAmount, string? comment)
        {
            StoreId = Guard.Against.NegativeOrZero(storeId);
            AgentId = Guard.Against.NegativeOrZero(agentId);
            Type = Guard.Against.EnumOutOfRange<OrderType>(type);
            Amount = Guard.Against.NegativeOrZero(amount);

            Date = date;
            DocId = docId;
            AuditCurrentAmount = auditCurrentAmount;
            AuditFactAmount = auditFactAmount;
            Comment = comment;
        }

        internal static Order Create(string? userName, string? userStoreName, long storeId, long managerId, string managerName, long agentId, string agentName, OrderType type, decimal amount, DateTime date, long? docId, string? comment, string storeName)
        {
            var order = new Order(storeId, agentId, type, amount, date, docId, comment);

            order.RegisterDomainEvent(new OrderHistoryEvent(order, null, HistoryMethod.Create, userName, userStoreName, storeName, agentName));

            order.RegisterDomainEvent(new AgentRemainsEvent(order, userStoreName!, agentName, managerId, managerName, HistoryMethod.Create));

            return order;
        }

        internal static Order Create(string? userName, string? userStoreName, long storeId, long agentId, string agentName, OrderType type, decimal amount, DateTime date, long? docId, decimal? auditCurrentAmount, decimal? auditFactAmount, string? comment, string storeName)
        {
            var order = new Order(storeId, agentId, type, amount, date, docId, auditCurrentAmount, auditFactAmount, comment);

            order.RegisterDomainEvent(new OrderHistoryEvent(order, null, HistoryMethod.Create, userName, userStoreName, storeName, agentName));

            return order;
        }

        internal void Update(string? userName, string? userStoreName, long storeId, long managerId, string managerName, long agentId, string agentName, OrderType type, decimal amount, DateTime date, long? docId, string? comment, string storeName, string oldAgentName)
        {
            var oldOrder = ToSnapshot();

            SetParams(storeId, agentId, type, amount, date, docId, default, default, comment);
            
            UpdateDate = DateTime.Now;

            RegisterDomainEvent(new OrderHistoryEvent(this, oldOrder, HistoryMethod.Update, userName, userStoreName, storeName, $"{oldAgentName}|{agentName}"));
            
            RegisterDomainEvent(new AgentRemainsEvent(this, userStoreName!, agentName, managerId, managerName, HistoryMethod.Update));
        }

        //internal void Update(string? userName, string? userStoreName, long storeId, long agentId, OrderType type, decimal amount, DateTime date, long? docId, decimal? auditCurrentAmount, decimal? auditFactAmount, string? comment, string storeName)
        //{
        //    var oldOrder = ToSnapshot();

        //    SetParams(storeId, agentId, type, amount, date, docId, auditCurrentAmount, auditFactAmount, comment);
            
        //    UpdateDate = DateTime.Now;

        //    RegisterDomainEvent(new OrderHistoryEvent(this, oldOrder, HistoryMethod.Update, userName, userStoreName, storeName, objectAgentName));
        //}
        internal void Delete(string? userName, string? userStoreName)
        {
            RegisterDomainEvent(new OrderHistoryEvent(this, null, HistoryMethod.Delete, userName, userStoreName, null, null));

            RegisterDomainEvent(new AgentRemainsEvent(this, userStoreName!, null, 0, null, HistoryMethod.Delete));
        }


        public void AddAgent(long agentId, string name, decimal debt, decimal difference, OrderType type)
        {
            if (type != OrderType.AgentRevision && debt <= 0) throw new ArgumentException("Debt must be > 0", nameof(debt));

            var existing = _agents.FirstOrDefault(p => p.AgentId == agentId);
            if (existing != null)
            {
                existing.UpdateDebt(debt);
                return;
            }

            var agent = new Agent(agentId, name, debt, difference);
            _agents.Add(agent);
        }

        public void RemoveAgent(long agentId)
        {
            var agent = _agents.FirstOrDefault(p => p.AgentId == agentId);
            if (agent == null) throw new InvalidOperationException("Agent not found");
            _agents.Remove(agent);
        }

        public void ChangeAgentDebt(long agentId, int newDebt)
        {
            var agent = _agents.FirstOrDefault(p => p.AgentId == agentId);
            if (agent == null) throw new InvalidOperationException("Agent not found");
            if (newDebt <= 0) RemoveAgent(agentId);
            else agent.SetDebt(newDebt);
        }

        public void UpdateAgent(long agentId, string name, decimal debt, decimal difference, OrderType type)
        {
            var agent = _agents.FirstOrDefault(p => p.AgentId == agentId);
            if (agent == null) throw new InvalidOperationException("Product not found");
            if (type != OrderType.AgentRevision && debt <= 0) RemoveAgent(agentId);
            else agent.UpdateAgent(agentId, name, debt, difference);
        }


        public sealed record OrderSnapshot(long Id, int Code, DateTime Date, long? DocId, long StoreId, long AgentId, decimal Amount, decimal? AuditCurrentAmount, decimal? AuditFactAmount, string? Comment, OrderType Type, DateTime CreateDate, DateTime UpdateDate);

        public OrderSnapshot ToSnapshot() =>
            new(Id, Code, Date, DocId, StoreId, AgentId, Amount, AuditCurrentAmount, AuditFactAmount, Comment, Type, CreateDate, UpdateDate);
    }
}
