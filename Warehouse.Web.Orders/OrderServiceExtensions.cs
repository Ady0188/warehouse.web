using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using Warehouse.Web.Orders.Data;

namespace Warehouse.Web.Orders
{
    public static class OrderServiceExtensions
    {
        public static IServiceCollection AddOrderModuleServices(this IServiceCollection services,
            ConfigurationManager config,
            ILogger logger,
            List<Assembly> mediatRAssemblies)
        {
            string? connectionString = config.GetConnectionString("OrderConnectionString");
            services.AddDbContext<OrderDbContext>(config => config.UseNpgsql(connectionString));

            services.AddScoped<IOrderRepository, EfOrderRepository>();
            services.AddScoped<IReadOnlyOrderRepository, EfOrderRepository>();

            // Add MediatR Domain Event Dispatcher
            services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
            services.AddScoped<ExportFileService>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

            mediatRAssemblies.Add(typeof(OrderServiceExtensions).Assembly);

            services.AddHostedService<MigrationsHostedService>();

            logger.Information("{Module} module services registered", "Orders");

            return services;
        }
    }
}