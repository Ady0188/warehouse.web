namespace Warehouse.Web.Orders
{
    internal interface IOrderRepository : IReadOnlyOrderRepository
    {
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Order order);
        Task SaveChangesAsync();
    }
}
