namespace Warehouse.Web.Managers;

internal interface IReadOnlyManagerRepository
{
    Task<Manager?> GetByIdAsync(long id);
    Task<List<Manager>> ListAsync();
    Task<(List<Manager> Result, int Total)> ListAsync(GetAllOptions options);
    Task<List<Manager>> ListByStoreIdsAsync(long[] storeIds);
    Task<bool> ExistsByNameAsync(string firstname, string lastname);
}
