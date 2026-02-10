using Ardalis.GuardClauses;

namespace Warehouse.Web.Stores;

internal class StoreService : IStoreService
{
    private readonly IStoreRepository _repository;

    public StoreService(IStoreRepository repository)
    {
        _repository = repository;
    }

    public async Task CreateStoreAsync(StoreDto newStore)
    {
        var store = new Store(newStore.Name);

        await _repository.AddAsync(store);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdateStoreAsync(StoreDto newStore)
    {
        var store = await _repository.GetByIdAsync(newStore.Id);

        if (store is null)
        {
            throw new NotFoundException(nameof(Store), newStore.Id.ToString());
        }

        store.UpdateName(newStore.Name);

        await _repository.UpdateAsync(store);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteStoreAsync(long id)
    {
        var store = await _repository.GetByIdAsync(id);

        if (store is null)
        {
            throw new NotFoundException(nameof(Store), id.ToString());
        }

        store.Delate();

        await _repository.UpdateAsync(store);
        await _repository.SaveChangesAsync();
    }

    public async Task<StoreDto> GetStoreByIdAsync(long id)
    {
        var store = await _repository.GetByIdAsync(id);

        // TODO: handle not found case
        if (store is null)
        {
            throw new NotFoundException(nameof(Store), id.ToString());
        }

        return new StoreDto(store!.Id, store.Name);
    }

    public async Task<List<StoreDto>> ListStoresAsync()
    {
        var stores = (await _repository.ListAsync())
          .Select(store => new StoreDto(store.Id, store.Name))
          .ToList();

        return stores;
    }

    public async Task<(List<StoreDto> Result, int Total)> ListStoresAsync(GetAllOptions options)
    {
        var list = await _repository.ListAsync(options);
        var stores = list.Result
          .Select(store => new StoreDto(store.Id, store.Name))
          .ToList();

        return (stores, list.Total);
    }
}
