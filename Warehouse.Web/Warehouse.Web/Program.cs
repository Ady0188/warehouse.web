using System.Reflection;
using Blazored.LocalStorage;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using Warehouse.Web.Client.Helpers;
using Warehouse.Web.Components;
using Warehouse.Web.Components.Account;
using Warehouse.Web.Users.Data;
using Warehouse.Web.Stores;
using Warehouse.Web.Managers;
using Warehouse.Web.Users;
using Warehouse.Web.Agents;
using Warehouse.Web.Catalog;
using Warehouse.Web.Reporting;
using Warehouse.Web.Orders;
using Warehouse.Web.Operations;
using Append.Blazor.Printing;

var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

logger.Information("Starting Warehouse Web Api...");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, congig) =>
  congig.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddScoped(sp =>
{
    var context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var client = new HttpClient { BaseAddress = new Uri($"{context!.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}") };

    return client;
});

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
});

builder.Services.AddScoped<IAppSnackbarService, AppSnackbarService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddScoped<IPrintingService, PrintingService>();

List<Assembly> mediatRAssembly = [typeof(Program).Assembly];
builder.Services.AddUserModuleServices(builder.Configuration, logger, mediatRAssembly);
builder.Services.AddStoreModuleServices(builder.Configuration, logger, mediatRAssembly);
builder.Services.AddManagerModuleServices(builder.Configuration, logger, mediatRAssembly);
builder.Services.AddAgentModuleServices(builder.Configuration, logger, mediatRAssembly);
builder.Services.AddProductModuleServices(builder.Configuration, logger, mediatRAssembly);
builder.Services.AddReportingModuleServices(builder.Configuration, logger, mediatRAssembly);
builder.Services.AddOrderModuleServices(builder.Configuration, logger, mediatRAssembly);
builder.Services.AddOperationModuleServices(builder.Configuration, logger, mediatRAssembly);

//builder.Services.AddAuthentication(options =>
//    {
//        //options.DefaultScheme = IdentityConstants.ApplicationScheme;
//        //options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

//        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
//        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
//        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
//    })
//    .AddIdentityCookies();

//builder.Services.AddAuthorization(); // <— добавили

builder.Services.AddFastEndpoints()
  .SwaggerDocument();

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(connectionString));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddRoles<IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddSignInManager()
//    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssembly.ToArray()));

builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// ВАЖНО: перед эндпоинтами
app.UseAuthentication();
app.UseAuthorization();

// ВАЖНО: до маппинга Razor/FastEndpoints эндпоинтов уже ок,
// но для FastEndpoints нужен сам middleware:
app.UseFastEndpoints()
  .UseSwaggerGen();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Warehouse.Web.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();