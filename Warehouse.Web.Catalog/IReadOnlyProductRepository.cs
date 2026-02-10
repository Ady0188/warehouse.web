namespace Warehouse.Web.Catalog;

internal interface IReadOnlyProductRepository
{
    Task<Product?> GetByIdAsync(long id);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByNameAndManufacturerAsync(string name, string? manufacturer);
    Task<List<Product>> ListAsync();
    Task<(List<Product> Result, int Total)> ListAsync(GetAllOptions options);
}
