using Microsoft.EntityFrameworkCore;

namespace Warehouse.Web.Stores.Data;

internal class EfStoreRepository : IStoreRepository
{
    private readonly StoreDbContext _dbContext;

    public EfStoreRepository(StoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Store store)
    {
        _dbContext.Add(store);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Store store)
    {
        _dbContext.Update(store);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Store store)
    {
        _dbContext.Remove(store);
        return Task.CompletedTask;
    }

    public async Task<Store?> GetByIdAsync(long id)
    {
        return await _dbContext.Stores.FindAsync(id);
    }

    public async Task<List<Store>> GetByIdsAsync(IEnumerable<long> ids)
    {
        return await _dbContext.Stores.Where(x => ids.Contains(x.Id) && x.DelatedDate == null).OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<List<Store>> ListAsync()
    {
        return await _dbContext.Stores.Where(x => x.DelatedDate == null).OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<(List<Store> Result, int Total)> ListAsync(GetAllOptions options)
    {
        var query = _dbContext.Stores
            .ApplyFilters(options)
            .ApplySorting(options);

        var total = await _dbContext.Stores
            .ApplyFilters(options)
            .CountAsync();

        var result = await query
            .Skip(options.Skip)
            .Take(options.PageSize)
            .ToListAsync();

        return (result, total);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}