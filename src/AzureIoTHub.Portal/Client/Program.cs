// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using AzureIoTHub.Portal.Shared.Settings;
    using Blazored.Modal;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor.Services;
    using Tewr.Blazor.FileReader;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                                                    /*.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()*/;

            builder.Services.AddFileReaderService(o => o.UseWasmSharedBuffer = true);

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));
            builder.Services.AddBlazoredModal();

            builder.Services.AddMudServices();

            await ConfigureOidc(builder);

            await builder.Build().RunAsync();
        }

        private static async Task ConfigureOidc(WebAssemblyHostBuilder builder)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            var settings = await httpClient.GetFromJsonAsync<OIDCSettings>("OIDCSettings");

            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.Authority = settings.Authority;
                options.ProviderOptions.MetadataUrl = settings.MetadataUrl;
                options.ProviderOptions.ClientId = settings.ClientId;

                options.ProviderOptions.DefaultScopes.Clear();
                options.ProviderOptions.DefaultScopes.Add($"profile openid {settings.Scope}");

                options.ProviderOptions.ResponseType = "id_token";
            });
        }
    }
}
