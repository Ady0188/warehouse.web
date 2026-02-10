using Microsoft.EntityFrameworkCore;

namespace Warehouse.Web.Catalog.Data;

internal class EFProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public EFProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Product product)
    {
        _context.Add(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product)
    {
        _context.Remove(product);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAndManufacturerAsync(string name, string? manufacturer)
    {
        if(string.IsNullOrWhiteSpace(manufacturer))
            return await ExistsByNameAsync(name);

        var normalizedName = name.Trim().ToLower();
        var normalizedManufacturer = manufacturer.Trim().ToLower();

        return await _context.Products.AnyAsync(p => p.Name.ToLower() == normalizedName && p.Description != null && p.Description.ToLower() == normalizedManufacturer);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        var normalized = name.Trim().ToLower();

        return await _context.Products.AnyAsync(p => p.Name.ToLower() == normalized);
    }

    public async Task<Product?> GetByIdAsync(long id)
    {
        return await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Product>> ListAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<(List<Product> Result, int Total)> ListAsync(GetAllOptions options)
    {
        var query = _context.Products
                .ApplyFilters(options)
                .ApplySorting(options);

        var count = await _context.Products
                .ApplyFilters(options)
                .CountAsync();

        var result = await query
            .Skip(options.Skip)
            .Take(options.PageSize)
            .ToListAsync();

        return (result, count);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(Product product)
    {
        _context.Update(product);
        return Task.CompletedTask;
    }
}
