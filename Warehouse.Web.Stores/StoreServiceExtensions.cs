using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Warehouse.Web.Stores.Data;

namespace Warehouse.Web.Stores;

public static class StoreServiceExtensions
{
    public static IServiceCollection AddStoreModuleServices(this IServiceCollection services,
      ConfigurationManager config,
      ILogger logger,
      List<System.Reflection.Assembly> mediatRAssemblies)
    {
        var connectionStirng = config.GetConnectionString("StoresConnectionString");
        services.AddDbContext<StoreDbContext>(options =>
        {
            options.UseNpgsql(connectionStirng);
        });

        services.AddScoped<IStoreRepository, EfStoreRepository>();
        services.AddScoped<IReadOnlyStoreRepository, EfStoreRepository>();
        services.AddScoped<IStoreService, StoreService>();

        mediatRAssemblies.Add(typeof(StoreServiceExtensions).Assembly);

        services.AddHostedService<MigrationsHostedService>();

        logger.Information("{Module} module services registered", "Stores");

        return services;
    }
}