
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Web.Managers.Data;

internal class EfManagerRepository : IManagerRepository
{
    private readonly ManagerDbContext _context;

    public EfManagerRepository(ManagerDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Manager manager)
    {
        _context.Add(manager);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Manager manager)
    {
        _context.Managers.Remove(manager);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(string firstname, string lastname)
    {
        return await _context.Managers.AnyAsync(x => x.Firstname == firstname && x.Lastname == lastname);
    }

    public async Task<Manager?> GetByIdAsync(long id)
    {
        return await _context.Managers.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Manager>> ListAsync()
    {
        return await _context.Managers.Where(x => x.DeleteDate == null).ToListAsync();
    }

    public async Task<(List<Manager> Result, int Total)> ListAsync(GetAllOptions options)
    {
        var result = await _context.Managers
            .ApplyFilters(options)
            .ApplySorting(options)
            .ToListAsync();

        var count = await _context.Managers
            .ApplyFilters(options)
            .CountAsync();

        return (result, count);
    }

    public async Task<List<Manager>> ListByStoreIdsAsync(long[] storeIds)
    {
        return await _context.Managers.Where(x => x.DeleteDate == null && storeIds.Contains(x.StoreId)).ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(Manager manager)
    {
        _context.Update(manager);
        return Task.CompletedTask;
    }
}
