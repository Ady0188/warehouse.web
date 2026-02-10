using Ardalis.GuardClauses;

namespace Warehouse.Web.Stores;

internal class Store
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    public DateTime? DelatedDate { get; set; }

    internal Store(long id, string name)
    {
        Id = Guard.Against.Default(id);
        Name = Guard.Against.NullOrEmpty(name);
    }
    internal Store(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);
    }

    internal void UpdateName(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);  
        UpdatedDate = DateTime.Now;
    }

    internal void Delate()
    {
        DelatedDate = DateTime.Now;
    }
}
