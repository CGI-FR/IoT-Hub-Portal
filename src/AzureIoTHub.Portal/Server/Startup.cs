// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure;
    using Azure.Storage.Blobs;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Identity;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Server.Wrappers;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
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
    using Prometheus;

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

            _ = services.Configure<ClientApiIndentityOptions>(opts =>
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

            _ = services.AddSingleton(configuration);

            _ = services.AddRazorPages();

            _ = services.AddScoped(t =>
            {
                return RegistryManager.CreateFromConnectionString(configuration.IoTHubConnectionString);
            });

            _ = services.AddScoped(t =>
            {
                return ServiceClient.CreateFromConnectionString(configuration.IoTHubConnectionString);
            });

            _ = services.AddScoped(t =>
            {
                return ProvisioningServiceClient.CreateFromConnectionString(configuration.DPSConnectionString);
            });

            _ = services.AddTransient<IProvisioningServiceClient, ProvisioningServiceClientWrapper>();
            _ = services.AddTransient(sp => new BlobServiceClient(configuration.StorageAccountConnectionString));
            _ = services.AddTransient<ITableClientFactory>(sp => new TableClientFactory(configuration.StorageAccountConnectionString));
            _ = services.AddTransient<IDeviceModelImageManager, DeviceModelImageManager>();
            _ = services.AddTransient<IDeviceService, DeviceService>();
            _ = services.AddTransient<IConcentratorTwinMapper, ConcentratorTwinMapper>();
            _ = services.AddTransient<IDeviceModelCommandMapper, DeviceModelCommandMapper>();
            _ = services.AddTransient<IDeviceModelCommandsManager, DeviceModelCommandsManager>();
            _ = services.AddTransient<IDeviceProvisioningServiceManager, DeviceProvisioningServiceManager>();

            _ = services.AddTransient<IDeviceTwinMapper<DeviceListItem, DeviceDetails>, DeviceTwinMapper>();
            _ = services.AddTransient<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>, LoRaDeviceTwinMapper>();
            _ = services.AddTransient<IDeviceModelMapper<DeviceModel, DeviceModel>, DeviceModelMapper>();
            _ = services.AddTransient<IDeviceModelMapper<DeviceModel, LoRaDeviceModel>, LoRaDeviceModelMapper>();
            _ = services.AddTransient<IDeviceTagMapper, DeviceTagMapper>();

            _ = services.AddTransient<IConfigService, ConfigService>();
            _ = services.AddTransient<IDeviceTagService, DeviceTagService>();

            _ = services.AddMudServices();

            var transientHttpErrorPolicy = HttpPolicyExtensions
                                    .HandleTransientHttpError()
                                    .OrResult(c => c.StatusCode == HttpStatusCode.NotFound)
                                    .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100));

            _ = services.AddHttpClient("RestClient")
                .AddPolicyHandler(transientHttpErrorPolicy);

            _ = services.AddHttpClient<ILoraDeviceMethodManager, LoraDeviceMethodManager>(client =>
            {
                client.BaseAddress = new Uri(configuration.LoRaKeyManagementUrl);
                client.DefaultRequestHeaders.Add("x-functions-key", configuration.LoRaKeyManagementCode);
                client.DefaultRequestHeaders.Add("api-version", "2020-10-09");
            })
                .AddPolicyHandler(transientHttpErrorPolicy);

            _ = services.AddHttpClient<IRouterConfigManager, RouterConfigManager>(client =>
            {
                client.BaseAddress = new Uri(configuration.LoRaRegionRouterConfigUrl);
            }).AddPolicyHandler(transientHttpErrorPolicy);

            _ = services.AddControllers();

            _ = services.AddEndpointsApiExplorer();

            _ = services.AddApplicationInsightsTelemetry();

            _ = services.AddSwaggerGen(opts =>
              {
                  opts.SwaggerDoc("v1", new OpenApiInfo
                  {
                      Version = "1.0",
                      Title = "Azure IoT Hub Portal API",
                      Description = "Available APIs for managing devices from Azure IoT Hub."
                  });

                  // using System.Reflection;
                  opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"AzureIoTHub.Portal.Server.xml"));
                  opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"AzureIoTHub.Portal.Shared.xml"));

                  opts.TagActionsBy(api => new[] { api.GroupName });
                  opts.DocInclusionPredicate((name, api) => true);

                  opts.DescribeAllParametersInCamelCase();

                  var securityDefinition = new OpenApiSecurityScheme()
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

                  var securityRequirements = new OpenApiSecurityRequirement()
                  {
                    { securityDefinition, Array.Empty<string>() },
                  };

                  opts.AddSecurityDefinition("Bearer", securityDefinition);

                  opts.AddSecurityRequirement(securityRequirements);

                  opts.OrderActionsBy(api => api.RelativePath);
                  opts.UseInlineDefinitionsForEnums();
              });

            _ = services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Version"));
            });

            var mapperConfig = new MapperConfiguration(mc =>
            {
                _ = mc.CreateMap(typeof(AsyncPageable<>), typeof(List<>));

                mc.AddProfile(new DevicePropertyProfile());
            });

            var mapper = mapperConfig.CreateMapper();
            _ = services.AddSingleton(mapper);

            _ = services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();

                _ = app.UseDeveloperExceptionPage();
                _ = app.UseSwagger();
                _ = app.UseSwaggerUI();
            }
            else
            {
                _ = app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                _ = app.UseHsts();
            }

            _ = app.UseHttpsRedirection();
            _ = app.UseBlazorFrameworkFiles();
            _ = app.UseStaticFiles();

            _ = app.UseRouting();

            _ = app.UseAuthentication();
            _ = app.UseAuthorization();
            _ = app.UseMetricServer();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapRazorPages();
                _ = endpoints.MapControllers();
                // endpoints.MapFallbackToFile("index.html");

                // Prevent the user from getting HTML when the controller can't be found.
                _ = endpoints.Map("api/{**slug}", this.HandleApiFallback);

                // If this is a request for a web page, just do the normal out-of-the-box behaviour.
                _ = endpoints.MapFallbackToFile("{**slug}", "index.html", new StaticFileOptions
                {
                    OnPrepareResponse = ctx => ctx.Context.Response.Headers.Add("Cache-Control", new StringValues("no-cache"))
                });

                _ = endpoints.MapHealthChecks("/healthz");
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
            protected const string DPSServiceEndpointKey = "IoTDPS:ServiceEndpoint";

            protected const string OIDCScopeKey = "OIDC:Scope";
            protected const string OIDCAuthorityKey = "OIDC:Authority";
            protected const string OIDCMetadataUrlKey = "OIDC:MetadataUrl";
            protected const string OIDCClientIdKey = "OIDC:ClientId";
            protected const string OIDCApiClientIdKey = "OIDC:ApiClientId";

            protected const string IsLoRaFeatureEnabledKey = "LoRaFeature:Enabled";

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

            internal abstract string DPSEndpoint { get; }

            internal abstract string StorageAccountConnectionString { get; }

            internal abstract string OIDCScope { get; }

            internal abstract string OIDCApiClientId { get; }

            internal abstract string OIDCClientId { get; }

            internal abstract string OIDCMetadataUrl { get; }

            internal abstract string OIDCAuthority { get; }

            internal abstract bool IsLoRaEnabled { get; }

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

            internal override string DPSEndpoint => this.config[DPSServiceEndpointKey];

            internal override string StorageAccountConnectionString => this.config.GetConnectionString(StorageAccountConnectionStringKey);

            internal override string OIDCScope => this.config[OIDCScopeKey];

            internal override string OIDCAuthority => this.config[OIDCAuthorityKey];

            internal override string OIDCMetadataUrl => this.config[OIDCMetadataUrlKey];

            internal override string OIDCClientId => this.config[OIDCClientIdKey];

            internal override string OIDCApiClientId => this.config[OIDCApiClientIdKey];

            internal override bool IsLoRaEnabled => bool.Parse(this.config[IsLoRaFeatureEnabledKey] ?? "true");

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

            internal override string DPSEndpoint => this.config[DPSServiceEndpointKey];

            internal override string StorageAccountConnectionString => this.config[StorageAccountConnectionStringKey];

            internal override string OIDCScope => this.config[OIDCScopeKey];

            internal override string OIDCAuthority => this.config[OIDCAuthorityKey];

            internal override string OIDCMetadataUrl => this.config[OIDCMetadataUrlKey];

            internal override string OIDCClientId => this.config[OIDCClientIdKey];

            internal override string OIDCApiClientId => this.config[OIDCApiClientIdKey];

            internal override bool IsLoRaEnabled => bool.Parse(this.config[IsLoRaFeatureEnabledKey] ?? "true");

            internal override string StorageAccountBlobContainerName => this.config[StorageAccountBlobContainerNameKey];

            internal override string StorageAccountBlobContainerPartitionKey => this.config[StorageAccountBlobContainerPartitionKeyKey];

            internal override string LoRaKeyManagementUrl => this.config[LoRaKeyManagementUrlKey];

            internal override string LoRaKeyManagementCode => this.config[LoRaKeyManagementCodeKey];

            internal override string LoRaRegionRouterConfigUrl => this.config[LoRaRegionRouterConfigUrlKey];
        }
    }
}
