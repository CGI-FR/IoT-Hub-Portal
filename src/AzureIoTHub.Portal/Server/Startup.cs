// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server
{
    using System;
    using System.Net;
    using System.Net.Http.Headers;
    using Azure.Data.Tables;
    using Azure.Storage.Blobs;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Server.Identity;
    using AzureIoTHub.Portal.Server.Interfaces;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;
    using Microsoft.Identity.Web;
    using MudBlazor.Services;
    using Polly;
    using Polly.Extensions.Http;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var msalSettings = this.Configuration.GetSection(MsalSettingsConstants.RootKey);

            services.Configure<ClientApiIndentityOptions>(opts =>
            {
                opts.Authority = new Uri(new Uri(msalSettings[MsalSettingsConstants.Instance]), $"{msalSettings[MsalSettingsConstants.Domain]}/{msalSettings[MsalSettingsConstants.SignUpSignInPolicyId]}").ToString();
                opts.ClientId = msalSettings[MsalSettingsConstants.ClientId];
                opts.ScopeUri = $"https://{msalSettings[MsalSettingsConstants.Domain]}/{msalSettings[MsalSettingsConstants.ApiClientId]}/{msalSettings[MsalSettingsConstants.ScopeName]}";
            });

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(
                 jwtOopts =>
                 {
                     jwtOopts.TokenValidationParameters.RoleClaimType = "extension_Role";
                 },
                 identityOpts =>
                 {
                     identityOpts.Instance = msalSettings[MsalSettingsConstants.Instance].ToString();
                     identityOpts.Domain = msalSettings[MsalSettingsConstants.Domain].ToString();
                     identityOpts.SignUpSignInPolicyId = msalSettings[MsalSettingsConstants.SignUpSignInPolicyId].ToString();
                     identityOpts.ClientId = msalSettings[MsalSettingsConstants.ApiClientId].ToString();
                 });

            services.AddControllersWithViews(opts =>
            {
                opts.Filters.Add<ApiRequiredScopeFilter>();
            });

            services.AddRazorPages();

            services.AddTransient(t =>
            {
                return RegistryManager.CreateFromConnectionString(t.GetService<IConfiguration>()["IoTHub:ConnectionString"]);
            });

            services.AddTransient(t =>
            {
                return ServiceClient.CreateFromConnectionString(t.GetService<IConfiguration>()["IoTHub:ConnectionString"]);
            });

            services.AddTransient(t =>
            {
                return ProvisioningServiceClient.CreateFromConnectionString(t.GetService<IConfiguration>()["IoTDPS:ConnectionString"]);
            });

            // storage account
            var connectionString = this.Configuration.GetSection("StorageAcount:ConnectionString");
            services.AddSingleton(new TableClient(connectionString.Value, "DeviceTemplates"));

            // Blob container
            services.AddSingleton(new BlobServiceClient(connectionString.Value));

            services.AddSingleton<IB2CExtensionHelper, B2CExtensionHelper>();
            services.AddSingleton<DevicesServices>();

            services.AddHttpClient("RestClient")
                    .AddPolicyHandler(HttpPolicyExtensions
                                        .HandleTransientHttpError()
                                        .OrResult(c => c.StatusCode == HttpStatusCode.NotFound)
                                        .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100)));

            services.AddSingleton(t =>
            {
                // Initialize the client credential auth provider
                IConfidentialClientApplication confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(msalSettings[MsalSettingsConstants.ApiClientId].ToString())
                    .WithTenantId(msalSettings[MsalSettingsConstants.TenantId].ToString())
                    .WithClientSecret(msalSettings[MsalSettingsConstants.ApiClientSecret].ToString())
                    .Build();

                IAuthenticationProvider authProvider = new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    try
                    {
                        // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                        var authResult = await confidentialClient.AcquireTokenForClient(new[]
                        {
                            "https://graph.microsoft.com/.default"
                        }).ExecuteAsync();

                        // Add the access token in the Authorization header of the API
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                });
                return new GraphServiceClient(authProvider);
            });

            services.AddMudServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
