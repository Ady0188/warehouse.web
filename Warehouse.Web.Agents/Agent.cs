using Ardalis.GuardClauses;
using System.ComponentModel.DataAnnotations.Schema;
using Warehouse.Web.Contracts;

namespace Warehouse.Web.Agents;

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
        short method, long objectId,
        string objectName, string? oldData, string newData)
    => new()
    {
        StoreName = storeName,
        UserName = userName,
        Method = method,
        ObjectId = objectId,
        ObjectName = objectName,
        OldData = oldData,
        NewData = newData
    };
}

internal class Agent : IHaveDomainEvents
{
    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public string? Address { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreateDate { get; private set; } = DateTime.Now;
    public DateTime UpdateDate { get; private set; } = DateTime.Now;
    public DateTime? DeleteDate { get; private set; }
    public decimal? Dolg { get; private set; } = 0;
    public long ManagerId { get; private set; }


    private List<DomainEventBase> _domainEvents = new();
    [NotMapped]
    public IEnumerable<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();
    protected void RegisterDomainEvent(DomainEventBase eventItem) => _domainEvents.Add(eventItem);
    void IHaveDomainEvents.ClearDomainEvents() => _domainEvents.Clear();


    private Agent(string name, long managerId, string? address = null, string? phone = null, string? comment = null)
    {
        Name = Guard.Against.NullOrEmpty(name).Trim();
        ManagerId = Guard.Against.NegativeOrZero(managerId);

        Address = address;
        Phone = phone;
        Comment = comment;
    }

    internal static Agent Create(string? userName, string? userStoreName, string name, long managerId, string storeName, string managerName)
    {
        var agent = new Agent(name, managerId);

        agent.RegisterDomainEvent(new AgentHistoryEvent(agent, null, HistoryMethod.Create, userName, userStoreName, storeName, managerName));

        return agent;
    }

    internal static Agent Create(string? userName, string? userStoreName, string name, long managerId, string? address, string? phone, string? comment, string storeName, string managerName)
    {
        var agent = new Agent(name, managerId, address, phone, comment);

        agent.RegisterDomainEvent(new AgentHistoryEvent(agent, null, HistoryMethod.Create, userName, userStoreName, storeName, managerName));

        return agent;
    }

    internal void Update(string? userName, string? userStoreName, string name, long managerId, string? address, string? phone, string? comment, string storeName, string managerName)
    {
        var oldAgent = ToSnapshot();
        Name = Guard.Against.NullOrEmpty(name).Trim();
        ManagerId = Guard.Against.NegativeOrZero(managerId);

        Address = address;
        Phone = phone;
        Comment = comment;

        UpdateDate = DateTime.Now;

        var newAgent = ToSnapshot();

        RegisterDomainEvent(new AgentHistoryEvent(this, oldAgent, HistoryMethod.Update, userName, userStoreName, storeName, managerName));
    }
    internal void Delete(string? userName, string? userStoreName)
    {
        DeleteDate = DateTime.Now;
        RegisterDomainEvent(new AgentHistoryEvent(this, null, HistoryMethod.Delete, userName, userStoreName, null, null));
    }

    public sealed record AgentSnapshot(long Id,string Name,string Phone,string? Address,string? Comment,DateTime CreateDate,DateTime UpdateDate,DateTime? DeleteDate,decimal? Dolg,long ManagerId);

    public AgentSnapshot ToSnapshot() =>
        new(Id, Name, Phone, Address, Comment, CreateDate, UpdateDate, DeleteDate, Dolg, ManagerId);
}
