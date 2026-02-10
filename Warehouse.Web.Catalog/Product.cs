using Ardalis.GuardClauses;
using System.ComponentModel.DataAnnotations.Schema;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Catalog;

internal class Product : IHaveDomainEvents
{
    public long Id { get; private set; }
    public int Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Unit { get; private set; }
    public decimal BuyPrice { get; private set; }
    public decimal SellPrice { get; private set; }
    public int LimitRemain { get; private set; }
    public DateTime CreateDate { get; private set; } = DateTime.Now;
    public DateTime UpdateDate { get; private set; } = DateTime.Now;

    
    private List<DomainEventBase> _domainEvents = new();
    [NotMapped]
    public IEnumerable<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();
    protected void RegisterDomainEvent(DomainEventBase eventItem) => _domainEvents.Add(eventItem);
    void IHaveDomainEvents.ClearDomainEvents() => _domainEvents.Clear();


    public Product(string name, string unit, decimal buyPrice, decimal sellPrice, int limitRemain, string? description = default)
    {
        SetParams(name, unit, buyPrice, sellPrice, limitRemain, description);
    }

    internal static Product Create(string? userName, string? userStoreName, string name, string unit, decimal buyPrice, decimal sellPrice, int limitRemain)
    {
        var product = new Product(name, unit, buyPrice, sellPrice, limitRemain);

        product.RegisterDomainEvent(new ProductHistoryEvent(product, null, HistoryMethod.Create, userName, userStoreName));

        return product;
    }

    internal static Product Create(string? userName, string? userStoreName, string name, string unit, decimal buyPrice, decimal sellPrice, int limitRemain, string? description)
    {
        var product = new Product(name, unit, buyPrice, sellPrice, limitRemain, description);

        product.RegisterDomainEvent(new ProductHistoryEvent(product, null, HistoryMethod.Create, userName, userStoreName));

        return product;
    }

    internal void Update(string? userName, string? userStoreName, string name, string unit, decimal buyPrice, decimal sellPrice, int limitRemain, string? description)
    {
        var oldProduct = ToSnapshot();
        SetParams(name, unit, buyPrice, sellPrice, limitRemain, description);
        UpdateDate = DateTime.Now;

        var newProduct = ToSnapshot();

        RegisterDomainEvent(new ProductHistoryEvent(this, oldProduct, HistoryMethod.Update, userName, userStoreName));
    }

    private void SetParams(string name, string unit, decimal buyPrice, decimal sellPrice, int limit, string? description = default)
    {
        Name = Guard.Against.NullOrEmpty(name).Trim();
        Unit = Guard.Against.NullOrEmpty(unit).Trim();
        BuyPrice = Guard.Against.NegativeOrZero(buyPrice);
        SellPrice = Guard.Against.NegativeOrZero(sellPrice);
        LimitRemain = Guard.Against.Negative(limit);

        Description = description;
    }

    internal void Delete(string? userName, string? userStoreName)
    {
        RegisterDomainEvent(new ProductHistoryEvent(this, null, HistoryMethod.Delete, userName, userStoreName));
    }

    public sealed record ProductSnapshot(
        long Id,
        int Code,
        string Name,
        string? Description,
        string Unit,
        decimal BuyPrice,
        decimal SellPrice,
        int LimitRemain,
        DateTime CreateDate,
        DateTime UpdateDate);

    public ProductSnapshot ToSnapshot() =>
        new(Id,Code,Name,Description,Unit,BuyPrice,SellPrice,LimitRemain,CreateDate,UpdateDate);
}