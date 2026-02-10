using Ardalis.GuardClauses;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Managers;
internal class Outbox
{
    public long Id { get; private set; }
    public string StoreName { get; private set; }
    public string UserName { get; private set; }
    public short Method { get; private set; }
    public long ObjectId { get; private set; }
    public string ObjectName { get; private set; }
    public string? OldData { get; private set; }
    public string NewData { get; private set; }
    public DateTime CreateDate { get; private set; } = DateTime.Now;

    private Outbox() { }

    public static Outbox FromEvent(
        string storeName, string userName,
        short method, long managerId,
        string objectName, string? oldData, string newData)
    => new()
    {
        StoreName = storeName,
        UserName = userName,
        Method = method,
        ObjectId = managerId,
        ObjectName = objectName,
        OldData = oldData,
        NewData = newData
    };
}
internal class Manager : IHaveDomainEvents
{
    public long Id { get; private set; }
    public string Firstname { get; private set; }
    public string Lastname { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public DateTime CreateDate { get; private set; } = DateTime.Now;
    public DateTime UpdateDate { get; private set; } = DateTime.Now;
    public DateTime? DeleteDate { get; private set; }
    public long StoreId { get; private set; }

    private List<DomainEventBase> _domainEvents = new();
    [NotMapped]
    public IEnumerable<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();
    protected void RegisterDomainEvent(DomainEventBase eventItem) => _domainEvents.Add(eventItem);
    void IHaveDomainEvents.ClearDomainEvents() => _domainEvents.Clear();


    private Manager(string firstname, string lastname, long storeId, string? address = null, string? phone = null)
    {
        Firstname = Guard.Against.NullOrEmpty(firstname).Trim();
        Lastname = Guard.Against.NullOrEmpty(lastname).Trim();
        StoreId = Guard.Against.NegativeOrZero(storeId);

        Address = address;
        Phone = phone;


    }

    internal static Manager Create(string? userName, string? userStoreName, string firstname, string lastname, long storeId, string storeName)
    {
        var manager = new Manager(firstname, lastname, storeId);

        manager.RegisterDomainEvent(new ManagerHistoryEvent(manager, null, HistoryMethod.Create, userName, userStoreName, storeName));

        return manager;
    }

    internal static Manager Create(string? fullName, string? userStoreName, string firstname, string lastname, long storeId, string? address, string? phone, string storeName)
    {
        var manager = new Manager(firstname, lastname, storeId, address, phone);

        manager.RegisterDomainEvent(new ManagerHistoryEvent(manager, null, HistoryMethod.Create, fullName, userStoreName, storeName));

        return manager;
    }

    public sealed record ManagerSnapshot(
        long Id, string Firstname, string Lastname,
        string? Address, string? Phone,
        DateTime CreateDate, DateTime UpdateDate,
        DateTime? DeleteDate, long StoreId);

    public ManagerSnapshot ToSnapshot() =>
        new(Id, Firstname, Lastname, Address, Phone, CreateDate, UpdateDate, DeleteDate, StoreId);

    internal void Update(string? userName, string? userStoreName, string firstname, string lastname, long storeId, string? address, string? phone, string storeName)
    {
        var oldManager = ToSnapshot();
        Firstname = Guard.Against.NullOrEmpty(firstname);
        Lastname = Guard.Against.NullOrEmpty(lastname);
        StoreId = Guard.Against.NegativeOrZero(storeId);

        Address = address;
        Phone = phone;

        RegisterDomainEvent(new ManagerHistoryEvent(this, oldManager, HistoryMethod.Update, userName, userStoreName, storeName));
    }
    internal void Delete(string? userName, string? userStoreName)
    {
        DeleteDate = DateTime.Now;
        RegisterDomainEvent(new ManagerHistoryEvent(this, null, HistoryMethod.Delete, userName, userStoreName, null));
    }
}
