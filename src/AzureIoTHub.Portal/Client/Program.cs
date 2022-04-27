// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Settings;
    using Blazored.LocalStorage;
    using Blazored.Modal;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor.Services;
    using Tewr.Blazor.FileReader;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            _ = builder.Services.AddHttpClient("api", client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
                client.DefaultRequestHeaders.Add("X-Version", "1.0");
            });

            /*.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();*/

            _ = builder.Services.AddFileReaderService(o => o.UseWasmSharedBuffer = true);

            // Supply HttpClient instances that include access tokens when making requests to the server project
            _ = builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"))
                .AddScoped<ClipboardService>();

            _ = builder.Services.AddBlazoredLocalStorage();
            _ = builder.Services.AddBlazoredModal();
            _ = builder.Services.AddMudServices();

            _ = builder.Services.AddScoped<IEdgeDeviceClientService, EdgeDeviceClientService>();

            await ConfigureOidc(builder);
            await ConfigurePortalSettings(builder);

            await builder.Build().RunAsync();
        }

        private static async Task ConfigureOidc(WebAssemblyHostBuilder builder)
        {
            using var httpClient = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            var settings = await httpClient.GetFromJsonAsync<OIDCSettings>("api/settings/oidc");

            _ = builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.Authority = settings.Authority;
                options.ProviderOptions.MetadataUrl = settings.MetadataUrl.ToString();
                options.ProviderOptions.ClientId = settings.ClientId;

                options.ProviderOptions.DefaultScopes.Clear();
                options.ProviderOptions.DefaultScopes.Add($"profile openid {settings.Scope}");

                options.ProviderOptions.ResponseType = "id_token";
            });
        }

        private static async Task ConfigurePortalSettings(WebAssemblyHostBuilder builder)
        {
            using var httpClient = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            var settings = await httpClient.GetFromJsonAsync<PortalSettings>("api/settings/portal");

            _ = builder.Services.AddSingleton(settings);
        }
    }
}
