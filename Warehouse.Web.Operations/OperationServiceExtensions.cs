using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using Warehouse.Web.Operations.Data;

namespace Warehouse.Web.Operations
{
    public static class OperationServiceExtensions
    {
        public static IServiceCollection AddOperationModuleServices(this IServiceCollection services,
            ConfigurationManager config,
            ILogger logger,
            List<Assembly> mediatRAssemblies)
        {
            string? connectionString = config.GetConnectionString("OperationConnectionString");
            services.AddDbContext<OperationDbContext>(config => config.UseNpgsql(connectionString));

            services.AddScoped<IOperationRepository, EfOperationRepository>();
            services.AddScoped<IReadOnlyOperationRepository, EfOperationRepository>();

            // Add MediatR Domain Event Dispatcher
            services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
            services.AddScoped<ExportFileService>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

            mediatRAssemblies.Add(typeof(OperationServiceExtensions).Assembly);

            services.AddHostedService<MigrationsHostedService>();

            logger.Information("{Module} module services registered", "Operations");

            return services;
        }
    }
}