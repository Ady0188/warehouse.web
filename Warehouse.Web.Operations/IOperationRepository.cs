namespace Warehouse.Web.Operations
{
    internal interface IOperationRepository : IReadOnlyOperationRepository
    {
        Task AddAsync(Operation operation);
        Task UpdateAsync(Operation operation);
        Task DeleteAsync(Operation operation);
        Task SaveChangesAsync();
    }
}
