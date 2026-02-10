namespace Warehouse.Web.Stores;
internal interface IStoreService
{
  Task<List<StoreDto>> ListStoresAsync();
  Task<(List<StoreDto> Result, int Total)> ListStoresAsync(GetAllOptions options);
  Task<StoreDto> GetStoreByIdAsync(long id);
  Task CreateStoreAsync(StoreDto newStore);
  Task UpdateStoreAsync(StoreDto newStore);
  Task DeleteStoreAsync(long id);
}
