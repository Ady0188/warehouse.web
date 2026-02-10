namespace Warehouse.Web.Stores;

internal interface IReadOnlyStoreRepository
{
  Task<List<Store>> GetByIdsAsync(IEnumerable<long> ids);
  Task<Store?> GetByIdAsync(long id);
  Task<List<Store>> ListAsync();
  Task<(List<Store> Result, int Total)> ListAsync(GetAllOptions options);
}
