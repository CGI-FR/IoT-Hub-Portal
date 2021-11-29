// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server
{
    using System;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using Azure.Storage.Blobs;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Server.Identity;
    using AzureIoTHub.Portal.Server.Interfaces;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;
    using Microsoft.Identity.Web;
    using MudBlazor.Services;
    using Polly;
    using Polly.Extensions.Http;

    public class Startup
    {
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.HostEnvironment = environment;
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = ConfigHandler.Create(this.HostEnvironment, this.Configuration);

            services.Configure<ClientApiIndentityOptions>(opts =>
            {
                opts.Authority = new Uri(new Uri(configuration.MsalInstance), $"{configuration.MsalDomain}/{configuration.MsalSignUpSignInPolicyId}").ToString();
                opts.ClientId = configuration.MsalClientId;
                opts.ScopeUri = $"https://{configuration.MsalDomain}/{configuration.MsalApiClientId}/{configuration.MsalScopeName}";
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
                     identityOpts.Instance = configuration.MsalInstance;
                     identityOpts.Domain = configuration.MsalDomain;
                     identityOpts.SignUpSignInPolicyId = configuration.MsalSignUpSignInPolicyId;
                     identityOpts.ClientId = configuration.MsalApiClientId;
                 });

            services.AddControllersWithViews(opts =>
            {
                opts.Filters.Add(new ApiRequiredScopeFilter(configuration));
            });

            services.AddRazorPages();

            services.AddTransient(t =>
            {
                return RegistryManager.CreateFromConnectionString(configuration.IoTHubConnectionString);
            });

            services.AddTransient(t =>
            {
                return ServiceClient.CreateFromConnectionString(configuration.IoTHubConnectionString);
            });

            services.AddTransient(t =>
            {
                return ProvisioningServiceClient.CreateFromConnectionString(configuration.DPSConnectionString);
            });

            services.AddTransient(sp => new BlobServiceClient(configuration.StorageAccountConnectionString));
            services.AddTransient<ITableClientFactory>(sp => new TableClientFactory(configuration.StorageAccountConnectionString));
            services.AddTransient<ISensorImageManager, SensorImageManager>();

            services.AddSingleton<IB2CExtensionHelper, B2CExtensionHelper>(sp => new B2CExtensionHelper(configuration));

            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IDeviceTwinMapper, DeviceTwinMapper>();
            services.AddScoped<ConfigsServices>();

            services.AddHttpClient("RestClient")
                    .AddPolicyHandler(HttpPolicyExtensions
                                        .HandleTransientHttpError()
                                        .OrResult(c => c.StatusCode == HttpStatusCode.NotFound)
                                        .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100)));

            services.AddSingleton(t =>
            {
                // Initialize the client credential auth provider
                IConfidentialClientApplication confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(configuration.MsalApiClientId)
                    .WithTenantId(configuration.MsalTenantId)
                    .WithClientSecret(configuration.MsalApiClientSecret)
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
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
                // endpoints.MapFallbackToFile("index.html");

                // Prevent the user from getting HTML when the controller can't be found.
                endpoints.Map("api/{**slug}", this.HandleApiFallback);

                // If this is a request for a web page, just do the normal out-of-the-box behaviour.
                endpoints.MapFallbackToFile("{**slug}", "index.html", new StaticFileOptions
                {
                    OnPrepareResponse = ctx => ctx.Context.Response.Headers.Add("Cache-Control", new StringValues("no-cache"))
                });
            });

            await app.ApplicationServices.GetService<ISensorImageManager>().InitializeDefaultImageBlob();
        }

        private Task HandleApiFallback(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }

        internal abstract class ConfigHandler
        {
            protected const string IoTHubConnectionStringKey = "IoTHub:ConnectionString";
            protected const string DPSConnectionStringKey = "IoTDPS:ConnectionString";

            protected const string MsalScopeNameKey = "MsalSettings:ScopeName";
            protected const string MsalInstanceKey = "MsalSettings:Instance";
            protected const string MsalClientIdKey = "MsalSettings:ClientId";
            protected const string MsalApiClientIdKey = "MsalSettings:ApiClientId";
            protected const string MsalTenantIdKey = "MsalSettings:TenantId";
            protected const string MsalApiClientSecretKey = "MsalSettings:ApiClientSecret";
            protected const string MsalDomainKey = "MsalSettings:Domain";
            protected const string MsalSignUpSignInPolicyIdKey = "MsalSettings:SignUpSignInPolicyId";
            protected const string MsalB2CExtensionAppIdKey = "MsalSettings:B2CExtensionAppId";

            protected const string StorageAccountConnectionStringKey = "StorageAccount:ConnectionString";
            protected const string StorageAccountBlobContainerNameKey = "StorageAccount:BlobContainerName";
            protected const string StorageAccountBlobContainerPartitionKeyKey = "StorageAccount:BlobContainerPartitionKey";

            internal static ConfigHandler Create(IWebHostEnvironment env, IConfiguration config)
            {
                if (env.IsProduction())
                {
                    return new ProductionConfigHandler(config);
                }

                return new DevelopmentConfigHandler(config);
            }

            internal abstract string IoTHubConnectionString { get; }

            internal abstract string DPSConnectionString { get; }

            internal abstract string StorageAccountConnectionString { get; }

            internal abstract string MsalScopeName { get; }

            internal abstract string MsalInstance { get; }

            internal abstract string MsalClientId { get; }

            internal abstract string MsalApiClientId { get; }

            internal abstract string MsalTenantId { get; }

            internal abstract string MsalApiClientSecret { get; }

            internal abstract string MsalDomain { get; }

            internal abstract string MsalSignUpSignInPolicyId { get; }

            internal abstract string MsalB2CExtensionAppId { get; }

            internal abstract string StorageAccountBlobContainerName { get; }

            internal abstract string StorageAccountBlobContainerPartitionKey { get; }
        }

        internal class ProductionConfigHandler : ConfigHandler
        {
            private readonly IConfiguration config;

            internal ProductionConfigHandler(IConfiguration config)
            {
                this.config = config;
            }

            internal override string IoTHubConnectionString => this.config.GetConnectionString(IoTHubConnectionStringKey);

            internal override string DPSConnectionString => this.config.GetConnectionString(DPSConnectionStringKey);

            internal override string StorageAccountConnectionString => this.config.GetConnectionString(StorageAccountConnectionStringKey);

            internal override string MsalScopeName => this.config[MsalScopeNameKey];

            internal override string MsalInstance => this.config[MsalInstanceKey];

            internal override string MsalClientId => this.config[MsalClientIdKey];

            internal override string MsalApiClientId => this.config[MsalApiClientIdKey];

            internal override string MsalTenantId => this.config[MsalTenantIdKey];

            internal override string MsalApiClientSecret => this.config.GetConnectionString(MsalApiClientSecretKey);

            internal override string MsalDomain => this.config[MsalDomainKey];

            internal override string MsalSignUpSignInPolicyId => this.config[MsalSignUpSignInPolicyIdKey];

            internal override string MsalB2CExtensionAppId => this.config[MsalB2CExtensionAppIdKey];

            internal override string StorageAccountBlobContainerName => this.config[StorageAccountBlobContainerNameKey];

            internal override string StorageAccountBlobContainerPartitionKey => this.config[StorageAccountBlobContainerPartitionKeyKey];
        }

        internal class DevelopmentConfigHandler : ConfigHandler
        {
            private readonly IConfiguration config;

            internal DevelopmentConfigHandler(IConfiguration config)
            {
                this.config = config;
            }

            internal override string IoTHubConnectionString => this.config[IoTHubConnectionStringKey];

            internal override string DPSConnectionString => this.config[DPSConnectionStringKey];

            internal override string StorageAccountConnectionString => this.config[StorageAccountConnectionStringKey];

            internal override string MsalScopeName => this.config[MsalScopeNameKey];

            internal override string MsalInstance => this.config[MsalInstanceKey];

            internal override string MsalClientId => this.config[MsalClientIdKey];

            internal override string MsalApiClientId => this.config[MsalApiClientIdKey];

            internal override string MsalTenantId => this.config[MsalTenantIdKey];

            internal override string MsalApiClientSecret => this.config[MsalApiClientSecretKey];

            internal override string MsalDomain => this.config[MsalDomainKey];

            internal override string MsalSignUpSignInPolicyId => this.config[MsalSignUpSignInPolicyIdKey];

            internal override string MsalB2CExtensionAppId => this.config[MsalB2CExtensionAppIdKey];

            internal override string StorageAccountBlobContainerName => this.config[StorageAccountBlobContainerNameKey];

            internal override string StorageAccountBlobContainerPartitionKey => this.config[StorageAccountBlobContainerPartitionKeyKey];
        }
    }
}
