
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Web.Agents.Data;

internal class EfAgentRepository : IAgentRepository
{
    private readonly AgentDbContext _context;

    public EfAgentRepository(AgentDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Agent agent)
    {
        _context.Add(agent);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Agent agent)
    {
        _context.Remove(agent);
        return Task.CompletedTask;
    }

    public async Task<Agent?> GetByIdAsync(long id)
    {
        return await _context.Agents.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Agent>> ListAsync()
    {
        return await _context.Agents.Where(x => x.DeleteDate == null).OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<(List<Agent> Result, int Total)> ListAsync(GetAllOptions options)
    {
        var result = await _context.Agents
            .ApplyFilters(options)
            .ApplySorting(options)
            .ToListAsync();

        var total = await _context.Agents
            .ApplyFilters(options)
            .CountAsync();

        return (result, total);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(Agent agent)
    {
        _context.Update(agent);
        return Task.CompletedTask;
    }
}
