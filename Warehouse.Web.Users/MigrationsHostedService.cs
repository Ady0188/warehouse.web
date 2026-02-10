using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Warehouse.Web.Users.Data;

namespace Warehouse.Web.Users;

internal class MigrationsHostedService : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger _logger;

    public MigrationsHostedService(IServiceProvider provider, ILogger logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();
        try
        {
            // Получаем internal DbContext через тип напрямую, т.к. мы в том же assembly
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _logger.Information("Applying Agent module migrations...");
            await db.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error migrating StoreDbContext");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}