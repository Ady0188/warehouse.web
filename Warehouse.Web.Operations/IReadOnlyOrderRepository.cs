namespace Warehouse.Web.Operations
{
    internal interface IReadOnlyOperationRepository
    {
        Task<Operation?> GetByIdAsync(long id);
        Task<Operation?> GetByParentIdAsync(long parentId);
        Task<List<Operation>> ListAsync();
        Task<(List<Operation> Result, int Total)> ListAsync(GetAllOptions options);
        Task<(List<Operation> Result, int Total, long ProductCount, decimal ProductAmount, decimal ProductDiscount, decimal ProductToPay)> ListWithTotalsAsync(GetAllOptions options);
        Task<List<Operation>> ListSendsNotRecivedByToStoreIdAsync(long storeId);
    }
}
