namespace Warehouse.Web.Agents;
internal interface IAgentRepository : IReadOnlyAgentRepository
{
    Task AddAsync(Agent agent);
    Task UpdateAsync(Agent agent);
    Task DeleteAsync(Agent agent);
    Task SaveChangesAsync();
}
