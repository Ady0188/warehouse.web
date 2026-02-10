using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using Warehouse.Web.Reporting.Integrations;

namespace Warehouse.Web.Reporting;

public static class ReportingServiceExtensions
{
    public static IServiceCollection AddReportingModuleServices(this IServiceCollection services,
        ConfigurationManager config,
        ILogger logger,
        List<Assembly> mediatRAssemblies)
    {
        //string? connectionString = config.GetConnectionString("ReportingConnectionString");
        //services.AddDbContext<ReporingDbContext>(config => config.UseNpgsql(connectionString));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        services.AddScoped<HistoryIngestionService>();
        services.AddScoped<AgentRemainsIngestionService>();
        services.AddScoped<ProductTurnoverIngestionService>();
        services.AddScoped<IReportService, ReportService>();

        var context = new CustomAssemblyLoadContext();
        context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));

        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

        mediatRAssemblies.Add(typeof(ReportingServiceExtensions).Assembly);

        logger.Information("{Module} module services registered", "Reporting");

        return services;
    }
}
