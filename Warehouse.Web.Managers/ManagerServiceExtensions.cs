using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using Warehouse.Web.Managers.Data;

namespace Warehouse.Web.Managers;

public static class ManagerServiceExtensions
{
    public static IServiceCollection AddManagerModuleServices(this IServiceCollection services,
        ConfigurationManager config,
        ILogger logger,
        List<Assembly> mediatRAssemblies)
    {
        string? connectionString = config.GetConnectionString("ManagerConnectionString");
        services.AddDbContext<ManagerDbContext>(config => config.UseNpgsql(connectionString));

        services.AddScoped<IManagerRepository, EfManagerRepository>();
        services.AddScoped<IReadOnlyManagerRepository, EfManagerRepository>();

        // Add MediatR Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        mediatRAssemblies.Add(typeof(ManagerServiceExtensions).Assembly);

        services.AddHostedService<MigrationsHostedService>();

        logger.Information("{Module} module services registered", "Managers");

        return services;
    }
}