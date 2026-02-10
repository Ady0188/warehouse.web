using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using Warehouse.Web.Agents;
using Warehouse.Web.Catalog.Data;

namespace Warehouse.Web.Catalog;

public static class ProductServiceExtensions
{
    public static IServiceCollection AddProductModuleServices(this IServiceCollection services,
        ConfigurationManager config,
        ILogger logger,
        List<Assembly> mediatRAssembly)
    {
        var connectionString = config.GetConnectionString("ProductConnectionString");
        services.AddDbContext<ProductDbContext>(option => option.UseNpgsql(connectionString));

        // Add Services/Repositories
        services.AddScoped<IProductRepository, EFProductRepository>();
        services.AddScoped<IReadOnlyProductRepository, EFProductRepository>();
        services.AddScoped<ExportFileService>();

        // Add MediatR Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        mediatRAssembly.Add(typeof(ProductServiceExtensions).Assembly);

        services.AddHostedService<MigrationsHostedService>();

        logger.Information("{Module} module services registered", "Products");

        return services;
    }
}
