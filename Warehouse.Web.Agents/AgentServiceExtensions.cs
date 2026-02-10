using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using Warehouse.Web.Agents.Data;

namespace Warehouse.Web.Agents;

public static class AgentServiceExtensions
{
    public static IServiceCollection AddAgentModuleServices(this IServiceCollection services,
        ConfigurationManager config,
        ILogger logger,
        List<Assembly> mediatRAssemblies)
    {
        string? connectionString = config.GetConnectionString("AgentConnectionString");
        services.AddDbContext<AgentDbContext>(config => config.UseNpgsql(connectionString));

        services.AddScoped<IAgentRepository, EfAgentRepository>();
        services.AddScoped<IReadOnlyAgentRepository, EfAgentRepository>();

        // Add MediatR Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.AddScoped<ExportFileService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        mediatRAssemblies.Add(typeof(AgentServiceExtensions).Assembly);

        services.AddHostedService<MigrationsHostedService>();

        logger.Information("{Module} module services registered", "Agents");

        return services;
    }
}
