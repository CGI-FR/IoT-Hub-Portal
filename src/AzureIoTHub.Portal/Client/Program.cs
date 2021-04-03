// Copyright (c) Kevin BEAUGRAND. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using AzureIoTHub.Portal.Shared.Settings;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                                                    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

            await ConfigureMsalAuthentication(builder);

            await builder.Build().RunAsync();
        }

        private static async Task ConfigureMsalAuthentication(WebAssemblyHostBuilder builder)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            var settings = await httpClient.GetFromJsonAsync<MSALSettings>("MSALSettings");

            Console.WriteLine(settings.Authority);

            builder.Services.AddMsalAuthentication(options =>
            {
                options.ProviderOptions.Authentication.Authority = settings.Authority;
                options.ProviderOptions.Authentication.ClientId = settings.ClientId;
                options.ProviderOptions.Authentication.ValidateAuthority = settings.ValidateAuthority;

                options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
                options.ProviderOptions.DefaultAccessTokenScopes.Add(settings.ScopeUri);
                options.ProviderOptions.LoginMode = "redirect";

                options.UserOptions.RoleClaim = "extension_Role";
            });
        }
    }
}
