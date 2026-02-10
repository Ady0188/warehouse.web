using Append.Blazor.Printing;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Warehouse.Web.Client;
using Warehouse.Web.Client.Helpers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;

    //config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.PreventDuplicates = false;
});

builder.Services.AddScoped<IAppSnackbarService, AppSnackbarService>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<IPrintingService, PrintingService>();

await builder.Build().RunAsync();
