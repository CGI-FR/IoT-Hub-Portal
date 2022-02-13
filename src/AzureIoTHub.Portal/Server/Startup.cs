// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Identity;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Primitives;
    using Microsoft.OpenApi.Models;
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
                opts.MetadataUrl = new Uri(configuration.OIDCMetadataUrl).ToString();
                opts.ClientId = configuration.OIDCClientId;
                opts.Scope = configuration.OIDCScope;
                opts.Authority = configuration.OIDCAuthority;
            });

            /*
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddOpenIdConnect(opts =>
                {
                    opts.Authority = configuration.OIDCAuthority;
                    opts.MetadataAddress = configuration.OIDCMetadataUrl;
                    opts.ClientId = configuration.OIDCApiClientId;
                });

            services.AddControllersWithViews(opts =>
            {
                opts.Filters.Add(new ApiRequiredScopeFilter(configuration));
            });
            */

            services.AddSingleton(configuration);

            services.AddRazorPages();

            services.AddScoped(t =>
            {
                return RegistryManager.CreateFromConnectionString(configuration.IoTHubConnectionString);
            });

            services.AddScoped(t =>
            {
                return ServiceClient.CreateFromConnectionString(configuration.IoTHubConnectionString);
            });

            services.AddScoped(t =>
            {
                return ProvisioningServiceClient.CreateFromConnectionString(configuration.DPSConnectionString);
            });

            services.AddTransient(sp => new BlobServiceClient(configuration.StorageAccountConnectionString));
            services.AddTransient<ITableClientFactory>(sp => new TableClientFactory(configuration.StorageAccountConnectionString));
            services.AddTransient<IDeviceModelImageManager, DeviceModelImageManager>();
            services.AddTransient<IDeviceService, DeviceService>();
            services.AddTransient<IDeviceTwinMapper, DeviceTwinMapper>();
            services.AddTransient<IConcentratorTwinMapper, ConcentratorTwinMapper>();
            services.AddTransient<IDeviceModelCommandMapper, DeviceModelCommandMapper>();
            services.AddTransient<IDeviceModelMapper, DeviceModelMapper>();
            services.AddTransient<IConnectionStringManager, ConnectionStringManager>();
            services.AddTransient<IDeviceModelCommandsManager, DeviceModelCommandsManager>();
            services.AddTransient<IDeviceProvisioningServiceManager, DeviceProvisioningServiceManager>();

            services.AddTransient<ConfigsServices>();

            services.AddMudServices();

            var transientHttpErrorPolicy = HttpPolicyExtensions
                                    .HandleTransientHttpError()
                                    .OrResult(c => c.StatusCode == HttpStatusCode.NotFound)
                                    .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100));

            services.AddHttpClient("RestClient")
                .AddPolicyHandler(transientHttpErrorPolicy);

            services.AddHttpClient<ILoraDeviceMethodManager, LoraDeviceMethodManager>(client =>
            {
                client.BaseAddress = new Uri(configuration.LoRaKeyManagementUrl);
                client.DefaultRequestHeaders.Add("x-functions-key", configuration.LoRaKeyManagementCode);
            })
                .AddPolicyHandler(transientHttpErrorPolicy);

            services.AddHttpClient<IRouterConfigManager, RouterConfigManager>(client =>
            {
                client.BaseAddress = new Uri(configuration.LoRaRegionRouterConfigUrl);
            }).AddPolicyHandler(transientHttpErrorPolicy);

            services.AddControllers();

            services.AddEndpointsApiExplorer();

            services.AddApplicationInsightsTelemetry();

            services.AddSwaggerGen(opts =>
            {
                opts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "1.0",
                    Title = "Azure IoT Hub Portal API",
                    Description = "Available APIs for managing devices from Azure IoT Hub."
                });

                // using System.Reflection;
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                opts.TagActionsBy(api => new[] { api.GroupName });
                opts.DocInclusionPredicate((name, api) => true);

                opts.DescribeAllParametersInCamelCase();

                OpenApiSecurityScheme securityDefinition = new OpenApiSecurityScheme()
                {
                    Name = "Bearer",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description =
@"
    Specify the authorization token got from your IDP as a header.
    > Ex: ``Authorization: Bearer * ***``",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                };

                OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
                {
                    { securityDefinition, new string[] { } },
                };

                opts.AddSecurityDefinition("Bearer", securityDefinition);

                opts.AddSecurityRequirement(securityRequirements);

                opts.OrderActionsBy(api => api.RelativePath);
            });

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Version"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
                app.UseSwagger();
                app.UseSwaggerUI();
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

            await app.ApplicationServices.GetService<IDeviceModelImageManager>().InitializeDefaultImageBlob();
        }

        private Task HandleApiFallback(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }

        public abstract class ConfigHandler
        {
            protected const string IoTHubConnectionStringKey = "IoTHub:ConnectionString";
            protected const string DPSConnectionStringKey = "IoTDPS:ConnectionString";
            protected const string DPSDefaultEnrollmentGroupKey = "IoTDPS:DefaultEnrollmentGroup";
            protected const string DPSLoRaEnrollmentGroupKey = "IoTDPS:LoRaEnrollmentGroup";

            protected const string OIDCScopeKey = "OIDC:Scope";
            protected const string OIDCAuthorityKey = "OIDC:Authority";
            protected const string OIDCMetadataUrlKey = "OIDC:MetadataUrl";
            protected const string OIDCClientIdKey = "OIDC:ClientId";
            protected const string OIDCApiClientIdKey = "OIDC:ApiClientId";

            protected const string StorageAccountConnectionStringKey = "StorageAccount:ConnectionString";
            protected const string StorageAccountBlobContainerNameKey = "StorageAccount:BlobContainerName";
            protected const string StorageAccountBlobContainerPartitionKeyKey = "StorageAccount:BlobContainerPartitionKey";

            protected const string LoRaKeyManagementUrlKey = "LoRaKeyManagement:Url";
            protected const string LoRaKeyManagementCodeKey = "LoRaKeyManagement:Code";
            protected const string LoRaRegionRouterConfigUrlKey = "LoRaRegionRouterConfig:Url";

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

            internal abstract string DPSDefaultEnrollmentGroup { get; }

            internal abstract string DPSLoRaEnrollmentGroup { get; }

            internal abstract string StorageAccountConnectionString { get; }

            internal abstract string OIDCScope { get; }

            internal abstract string OIDCApiClientId { get; }

            internal abstract string OIDCClientId { get; }

            internal abstract string OIDCMetadataUrl { get; }

            internal abstract string OIDCAuthority { get; }

            internal abstract string StorageAccountBlobContainerName { get; }

            internal abstract string StorageAccountBlobContainerPartitionKey { get; }

            internal abstract string LoRaKeyManagementUrl { get; }

            internal abstract string LoRaKeyManagementCode { get; }

            internal abstract string LoRaRegionRouterConfigUrl { get; }
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

            internal override string DPSDefaultEnrollmentGroup => this.config[DPSDefaultEnrollmentGroupKey];

            internal override string DPSLoRaEnrollmentGroup => this.config[DPSLoRaEnrollmentGroupKey];

            internal override string StorageAccountConnectionString => this.config.GetConnectionString(StorageAccountConnectionStringKey);

            internal override string OIDCScope => this.config[OIDCScopeKey];

            internal override string OIDCAuthority => this.config[OIDCAuthorityKey];

            internal override string OIDCMetadataUrl => this.config[OIDCMetadataUrlKey];

            internal override string OIDCClientId => this.config[OIDCClientIdKey];

            internal override string OIDCApiClientId => this.config[OIDCApiClientIdKey];

            internal override string StorageAccountBlobContainerName => this.config[StorageAccountBlobContainerNameKey];

            internal override string StorageAccountBlobContainerPartitionKey => this.config[StorageAccountBlobContainerPartitionKeyKey];

            internal override string LoRaKeyManagementUrl => this.config[LoRaKeyManagementUrlKey];

            internal override string LoRaKeyManagementCode => this.config.GetConnectionString(LoRaKeyManagementCodeKey);

            internal override string LoRaRegionRouterConfigUrl => this.config[LoRaRegionRouterConfigUrlKey];
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

            internal override string DPSDefaultEnrollmentGroup => this.config[DPSDefaultEnrollmentGroupKey];

            internal override string DPSLoRaEnrollmentGroup => this.config[DPSLoRaEnrollmentGroupKey];

            internal override string StorageAccountConnectionString => this.config[StorageAccountConnectionStringKey];

            internal override string OIDCScope => this.config[OIDCScopeKey];

            internal override string OIDCAuthority => this.config[OIDCAuthorityKey];

            internal override string OIDCMetadataUrl => this.config[OIDCMetadataUrlKey];

            internal override string OIDCClientId => this.config[OIDCClientIdKey];

            internal override string OIDCApiClientId => this.config[OIDCApiClientIdKey];

            internal override string StorageAccountBlobContainerName => this.config[StorageAccountBlobContainerNameKey];

            internal override string StorageAccountBlobContainerPartitionKey => this.config[StorageAccountBlobContainerPartitionKeyKey];

            internal override string LoRaKeyManagementUrl => this.config[LoRaKeyManagementUrlKey];

            internal override string LoRaKeyManagementCode => this.config[LoRaKeyManagementCodeKey];

            internal override string LoRaRegionRouterConfigUrl => this.config[LoRaRegionRouterConfigUrlKey];
        }
    }
}
