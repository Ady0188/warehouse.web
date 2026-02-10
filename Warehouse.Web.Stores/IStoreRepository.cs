
namespace Warehouse.Web.Stores;

internal interface IStoreRepository : IReadOnlyStoreRepository
{
  Task AddAsync(Store store);
  Task UpdateAsync(Store store);
  Task DeleteAsync(Store store);
  Task SaveChangesAsync();
}
