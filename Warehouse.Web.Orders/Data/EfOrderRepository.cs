
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Web.Orders.Data
{
    internal class EfOrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public EfOrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(Order order)
        {
            _context.Add(order);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Order order)
        {
            _context.Remove(order);
            return Task.CompletedTask;
        }

        public async Task<Order?> GetByIdAsync(long id)
        {
            return await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Order>> ListAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<(List<Order> Result, int Total)> ListAsync(GetAllOptions options)
        {
            var result = await _context.Orders
                .ApplyFilters(options)
                .ApplySorting(options)
                .ToListAsync();

            var total = await _context.Orders
                .ApplyFilters(options)
                .CountAsync();

            return (result, total);
        }

        public async Task<(List<Order> Result, int Total, decimal TotalAmount)> ListWithTotalAsync(GetAllOptions options)
        {
            var result = await _context.Orders
                .ApplyFilters(options)
                .ApplySorting(options)
                .ToListAsync();

            var total = await _context.Orders
                .ApplyFilters(options)
                .CountAsync();

            var amount = await _context.Orders
                .ApplyFilters(options)
                .SumAsync(x => x.Amount);

            return (result, total, amount);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Order order)
        {
            _context.Update(order);
            return Task.CompletedTask;
        }
    }
}
