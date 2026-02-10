namespace Warehouse.Web.Agents;

internal interface IReadOnlyAgentRepository
{
    Task<Agent?> GetByIdAsync(long id);
    Task<List<Agent>> ListAsync();
    Task<(List<Agent> Result, int Total)> ListAsync(GetAllOptions options);
}
