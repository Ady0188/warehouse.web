using Ardalis.GuardClauses;
using System.ComponentModel.DataAnnotations.Schema;
using Warehouse.Web.Contracts;
using static Warehouse.Web.Operations.Product;

namespace Warehouse.Web.Operations;

internal class Operation : IHaveDomainEvents
{
    public long Id { get; private set; }
    public int Code { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Discount { get; private set; }
    public OperationType Type { get; private set; }
    public string? Comment { get; private set; }
    public long StoreId { get; private set; }
    public long ToStoreId { get; private set; }
    public long AgentId { get; private set; }
    public long ParentId { get; private set; }
    public bool IsReceived { get; private set; }

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();
    
    public DateTime CreateDate { get; private set; } = DateTime.Now;
    public DateTime UpdateDate { get; private set; } = DateTime.Now;



    private List<DomainEventBase> _domainEvents = new();
    [NotMapped]
    public IEnumerable<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();
    protected void RegisterDomainEvent(DomainEventBase eventItem) => _domainEvents.Add(eventItem);
    void IHaveDomainEvents.ClearDomainEvents() => _domainEvents.Clear();


    private Operation(DateTime date, decimal amount, decimal discount, OperationType type, long storeId, long toStoreId, long agentId, string? comment, long parentId)
    {
        SetParams(date, amount, discount, type, storeId, toStoreId, agentId, comment);

        if (type == OperationType.Receive)
        {
            IsReceived = true;
            ParentId = Guard.Against.NegativeOrZero(parentId);
        }
    }
    private Operation(DateTime date, decimal amount, decimal discount, OperationType type, long storeId, long toStoreId, long agentId, long parentId)
    {
        SetParams(date, amount, discount, type, storeId, toStoreId, agentId, default);

        if (type == OperationType.Receive)
        {
            IsReceived = true;
            ParentId = Guard.Against.NegativeOrZero(parentId);
        }
    }

    private void SetParams(DateTime date, decimal amount, decimal discount, OperationType type, long storeId, long toStoreId, long agentId, string? comment)
    {
        Discount = Guard.Against.Negative(discount);
        Type = Guard.Against.EnumOutOfRange<OperationType>(type);
        StoreId = Guard.Against.NegativeOrZero(storeId);

        ToStoreId = toStoreId;        
        if (type == OperationType.Send || type == OperationType.Receive)
            ToStoreId = Guard.Against.NegativeOrZero(toStoreId);

        Amount = amount;
        AgentId = agentId;
        if (type != OperationType.Audit && type != OperationType.Send && type != OperationType.Receive)
        {
            Amount = Guard.Against.NegativeOrZero(amount);
            AgentId = Guard.Against.NegativeOrZero(agentId);
        }

        Comment = comment;
        Date = date;
    }

    internal static Operation Create(string? userName, string? userStoreName, DateTime date, decimal amount, decimal discount, OperationType type, long parentId, long storeId, long toStoreId, string toStoreName, long managerId, string managerName, string managerPhone, long agentId, string agentName, string agentPhone, string agentAddress, string? comment, string storeName)
    {
        var operation = new Operation(date, amount, discount, type, storeId, toStoreId, agentId, comment, parentId);

        operation.RegisterDomainEvent(new OperationHistoryEvent(operation, null, HistoryMethod.Create, userName, userStoreName, storeName, agentName));

        operation.RegisterDomainEvent(new OperationReportEvent(operation, userStoreName!, toStoreName, agentName, agentPhone, agentAddress, managerId, managerName, managerPhone, OperationMethod.Create));

        return operation;
    }

    internal static Operation Create(string? userName, string? userStoreName, DateTime date, decimal amount, decimal discount, OperationType type, long parentId, long storeId, long toStoreId, string toStoreName, long managerId, string managerName, string managerPhone, long agentId, string agentName, string agentPhone, string agentAddress, string storeName)
    {
        var operation = new Operation(date, amount, discount, type, storeId, toStoreId, agentId, parentId);

        operation.RegisterDomainEvent(new OperationHistoryEvent(operation, null, HistoryMethod.Create, userName, userStoreName, storeName, agentName));

        operation.RegisterDomainEvent(new OperationReportEvent(operation, userStoreName!, toStoreName, agentName, agentPhone, agentAddress, managerId, managerName, managerPhone, OperationMethod.Create));

        return operation;
    }

    internal void Update(string? userName, string? userStoreName, DateTime date, decimal amount, decimal discount, OperationType type, long parentId, long storeId, long toStoreId, string toStoreName, long managerId, string managerName, string managerPhone, long agentId, string agentName, string agentPhone, string agentAddress, string storeName, string oldAgentName)
    {
        var oldOperation = ToSnapshot();

        SetParams(date, amount, discount, type, storeId, toStoreId, agentId, default);

        UpdateDate = DateTime.Now;

        if (type == OperationType.Send && parentId != 0)
            IsReceived = true;

        RegisterDomainEvent(new OperationHistoryEvent(this, oldOperation, HistoryMethod.Update, userName, userStoreName, storeName, $"{oldAgentName}|{agentName}"));

        RegisterDomainEvent(new OperationReportEvent(this, userStoreName!, toStoreName, agentName, agentPhone, agentAddress, managerId, managerName, managerPhone, OperationMethod.Update));
    }

    internal void Update(string? userName, string? userStoreName, DateTime date, decimal amount, decimal discount, OperationType type, long parentId, long storeId, long toStoreId, string toStoreName, long managerId, string managerName, string managerPhone, long agentId, string agentName, string agentPhone, string agentAddress, string? comment, string storeName, string oldAgentName)
    {
        var oldOperation = ToSnapshot();

        SetParams(date, amount, discount, type, storeId, toStoreId, agentId, comment);

        UpdateDate = DateTime.Now;
        if (type == OperationType.Send && parentId != 0)
            IsReceived = true;

        RegisterDomainEvent(new OperationHistoryEvent(this, oldOperation, HistoryMethod.Update, userName, userStoreName, storeName, $"{oldAgentName}|{agentName}"));

        RegisterDomainEvent(new OperationReportEvent(this, userStoreName!, toStoreName, agentName, agentPhone, agentAddress, managerId, managerName, managerPhone, OperationMethod.Update));
    }
    internal void Delete(string? userName, string? userStoreName)
    {
        RegisterDomainEvent(new OperationHistoryEvent(this, null, HistoryMethod.Delete, userName, userStoreName, null, null));

        RegisterDomainEvent(new OperationReportEvent(this, userStoreName!, string.Empty, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, OperationMethod.Delete));
    }


    public void AddProduct(long productId, int productCode, string name, decimal price, decimal buyPrice, decimal sellPrice, int quantity, string manufacturer, string unit, int difference, OperationType type)
    {
        if (type != OperationType.Audit && quantity <= 0) throw new ArgumentException("Quantity must be > 0", nameof(quantity));

        var existing = _products.FirstOrDefault(p => p.ProductId == productId);
        if (existing != null)
        {
            existing.IncreaseQuantity(quantity);
            return;
        }

        var product = new Product(productId, productCode, name, price, buyPrice, sellPrice, quantity, manufacturer, unit, difference);
        _products.Add(product);
    }

    public void RemoveProduct(long productId)
    {
        var prod = _products.FirstOrDefault(p => p.ProductId == productId);
        if (prod == null) throw new InvalidOperationException("Product not found");
        _products.Remove(prod);
    }

    public void ChangeProductQuantity(long productId, int newQuantity)
    {
        var prod = _products.FirstOrDefault(p => p.ProductId == productId);
        if (prod == null) throw new InvalidOperationException("Product not found");
        if (newQuantity <= 0) RemoveProduct(productId);
        else prod.SetQuantity(newQuantity);
    }

    public void UpdateProduct(long productId, int productCode, string name, decimal price, decimal buyPrice, decimal sellPrice, int quantity, string manufacturer, string unit, int difference, OperationType type)
    {
        var prod = _products.FirstOrDefault(p => p.ProductId == productId);
        if (prod == null) throw new InvalidOperationException("Product not found");
        if (type != OperationType.Audit && quantity <= 0) RemoveProduct(productId);
        else prod.UpdateProduct(productId, productCode, name, price, buyPrice, sellPrice, quantity, manufacturer, unit, difference, type);
    }


    public sealed record OperationSnapshot(long Id, int Code, DateTime Date, decimal Amount, decimal Discount, OperationType Type, long StoreId, long ToStoreId, long AgentId, string? Comment, List<ProductSnapshot> Products, DateTime CreateDate, DateTime UpdateDate);

    public OperationSnapshot ToSnapshot() =>
        new(Id, Code, Date, Amount, Discount, Type, StoreId, ToStoreId, AgentId, Comment, _products.Select(x => x.ToSnapshot()).ToList(), CreateDate, UpdateDate);
}
