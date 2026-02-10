namespace Warehouse.Web.Managers;
internal interface IManagerRepository : IReadOnlyManagerRepository
{
    Task AddAsync(Manager manager);
    Task UpdateAsync(Manager manager);
    Task DeleteAsync(Manager manager);
    Task SaveChangesAsync();
}
