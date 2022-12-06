// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using AzureIoTHub.Portal.Client;
using AzureIoTHub.Portal.Client.Handlers;
using AzureIoTHub.Portal.Client.Services;
using AzureIoTHub.Portal.Models.v10;
using AzureIoTHub.Portal.Settings;
using Blazored.LocalStorage;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Tewr.Blazor.FileReader;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

_ = builder.Services.AddTransient<ProblemDetailsHandler>();

_ = builder.Services.AddHttpClient("api", (sp, client) =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Add("X-Version", "1.0");
    _ = client.EnableIntercept(sp);
}).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
  .AddHttpMessageHandler<ProblemDetailsHandler>();

_ = builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

using var httpClient = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var settings = await httpClient.GetFromJsonAsync<OIDCSettings>("api/settings/oidc");

_ = builder.Services.Configure<OIDCSettings>(opts =>
{
    opts.ClientId = settings.ClientId;
    opts.MetadataUrl = settings.MetadataUrl;
    opts.Authority = settings.Authority;
    opts.Scope = settings.Scope;
});

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = settings.Authority;
    options.ProviderOptions.MetadataUrl = settings.MetadataUrl.ToString();
    options.ProviderOptions.ClientId = settings.ClientId;
    options.ProviderOptions.ResponseType = "code";

    options.ProviderOptions.DefaultScopes.Add(settings.Scope);
});

builder.Services.AddApiAuthorization();

_ = builder.Services.AddFileReaderService(o => o.UseWasmSharedBuffer = true);

_ = builder.Services.AddBlazoredLocalStorage();
_ = builder.Services.AddBlazoredModal();
_ = builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = false;
});

_ = builder.Services.AddScoped<ILayoutService, LayoutService>();
_ = builder.Services.AddScoped<IDashboardLayoutService, DashboardLayoutService>();
_ = builder.Services.AddSingleton<IDeviceLayoutService, DeviceLayoutService>();
_ = builder.Services.AddSingleton<IEdgeDeviceLayoutService, EdgeDeviceLayoutService>();

_ = builder.Services.AddScoped<IEdgeModelClientService, EdgeModelClientService>();
_ = builder.Services.AddScoped<IEdgeDeviceClientService, EdgeDeviceClientService>();
_ = builder.Services.AddScoped<IDashboardMetricsClientService, DashboardMetricsClientService>();
_ = builder.Services.AddScoped<IDeviceConfigurationsClientService, DeviceConfigurationsClientService>();
_ = builder.Services.AddScoped<IDeviceModelsClientService, DeviceModelsClientService>();
_ = builder.Services.AddScoped<IDeviceTagSettingsClientService, DeviceTagSettingsClientService>();
_ = builder.Services.AddScoped<ILoRaWanConcentratorClientService, LoRaWanConcentratorClientService>();
_ = builder.Services.AddScoped<IDeviceClientService, DeviceClientService>();
_ = builder.Services.AddScoped<ILoRaWanDeviceModelsClientService, LoRaWanDeviceModelsClientService>();
_ = builder.Services.AddScoped<ILoRaWanDeviceClientService, LoRaWanDeviceClientService>();
_ = builder.Services.AddScoped<IEdgeDeviceConfigurationsClientService, EdgeDeviceConfigurationsClientService>();
_ = builder.Services.AddScoped<IIdeaClientService, IdeaClientService>();
_ = builder.Services.AddScoped<ClipboardService>();

await ConfigurePortalSettings(builder);

// Enable loading bar
builder.Services.AddLoadingBar(options =>
{
    options.LoadingBarColor = "#D3E24A";
});
_ = builder.UseLoadingBar();

await builder.Build().RunAsync();

static async Task ConfigurePortalSettings(WebAssemblyHostBuilder builder)
{
    using var httpClient = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var settings = await httpClient.GetFromJsonAsync<PortalSettings>("api/settings/portal");

    _ = builder.Services.AddSingleton(settings);
}
