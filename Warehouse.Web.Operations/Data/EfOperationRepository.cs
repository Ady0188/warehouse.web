using Microsoft.EntityFrameworkCore;

namespace Warehouse.Web.Operations.Data;

internal class EfOperationRepository : IOperationRepository
{
    private readonly OperationDbContext _context;

    public EfOperationRepository(OperationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Operation operation)
    {
        _context.Add(operation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Operation operation)
    {
        _context.Remove(operation);
        return Task.CompletedTask;
    }

    public async Task<Operation?> GetByIdAsync(long id)
    {
        return await _context.Operations
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Operation?> GetByParentIdAsync(long parentId)
    {
        return await _context.Operations
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.ParentId == parentId);
    }

    public async Task<List<Operation>> ListAsync()
    {
        return await _context.Operations.ToListAsync();
    }

    public async Task<(List<Operation> Result, int Total)> ListAsync(GetAllOptions options)
    {
        var result = await _context.Operations
            .Include(Operation => Operation.Products)
                .ApplyFilters(options)
                .ApplySorting(options)
                .ToListAsync();

        var total = await _context.Operations
            .ApplyFilters(options)
            .CountAsync();

        return (result, total);
    }

    public async Task<(List<Operation> Result, int Total, long ProductCount, decimal ProductAmount, decimal ProductDiscount, decimal ProductToPay)> ListWithTotalsAsync(GetAllOptions options)
    {
        var result = await _context.Operations
            .Include(Operation => Operation.Products)
                .ApplyFilters(options)
                .ApplySorting(options)
                .ToListAsync();

        var total = await _context.Operations
            .ApplyFilters(options)
            .CountAsync();

        var count = await _context.Operations
            .ApplyFilters(options)
            .SumAsync(x => (long)x.Products.Sum(p => p.Quantity));

        var amount = await _context.Operations
            .ApplyFilters(options)
            .SumAsync(x => x.Amount);

        var discount = await _context.Operations
            .ApplyFilters(options)
            .SumAsync(x => x.Amount * x.Discount / 100);

        var topay = await _context.Operations
            .ApplyFilters(options)
            .SumAsync(x => x.Amount - (x.Amount * x.Discount / 100));

        return (result, total, count, amount, discount, topay);
    }

    public async Task<List<Operation>> ListSendsNotRecivedByToStoreIdAsync(long storeId)
    {
        return await _context.Operations
            .Include(Operation => Operation.Products)
            .Where(x => x.ToStoreId == storeId && x.Type == OperationType.Send && !x.IsReceived)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(Operation operation)
    {
        _context.Update(operation);
        return Task.CompletedTask;
    }
}
