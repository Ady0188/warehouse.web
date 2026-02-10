namespace Warehouse.Web.Catalog;
internal interface IProductRepository : IReadOnlyProductRepository
{
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task SaveChangesAsync();
}
