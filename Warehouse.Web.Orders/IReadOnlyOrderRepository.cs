namespace Warehouse.Web.Orders
{
    internal interface IReadOnlyOrderRepository
    {
        Task<Order?> GetByIdAsync(long id);
        Task<List<Order>> ListAsync();
        Task<(List<Order> Result, int Total)> ListAsync(GetAllOptions options);
        Task<(List<Order> Result, int Total, decimal TotalAmount)> ListWithTotalAsync(GetAllOptions options);
    }
}
