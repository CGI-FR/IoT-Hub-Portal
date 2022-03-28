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
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Identity;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Server.ServicesHealthCheck;
    using AzureIoTHub.Portal.Server.Wrappers;
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
            HostEnvironment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostEnvironment { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            var configuration = ConfigHandler.Create(HostEnvironment, Configuration);

            _ = services.Configure<ClientApiIndentityOptions>(opts =>
            {
                opts.MetadataUrl = new Uri(configuration.OIDCMetadataUrl);
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

            _ = services.AddScoped(_ => RegistryManager.CreateFromConnectionString(configuration.IoTHubConnectionString));

            _ = services.AddScoped(_ => ServiceClient.CreateFromConnectionString(configuration.IoTHubConnectionString));

            _ = services.AddScoped(_ => ProvisioningServiceClient.CreateFromConnectionString(configuration.DPSConnectionString));

            _ = services.AddTransient<IProvisioningServiceClient, ProvisioningServiceClientWrapper>();
            _ = services.AddTransient(_ => new BlobServiceClient(configuration.StorageAccountConnectionString));
            _ = services.AddTransient<ITableClientFactory>(_ => new TableClientFactory(configuration.StorageAccountConnectionString));
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
                                    .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100));

            _ = services.AddHttpClient("RestClient")
                .AddPolicyHandler(transientHttpErrorPolicy);

            _ = services.AddHttpClient<ILoraDeviceMethodManager, LoraDeviceMethodManager>(client =>
            {
                client.BaseAddress = new Uri(configuration.LoRaKeyManagementUrl);
                client.DefaultRequestHeaders.Add("x-functions-key", configuration.LoRaKeyManagementCode);
                client.DefaultRequestHeaders.Add("api-version", "2020-10-09");
            })
                .AddPolicyHandler(transientHttpErrorPolicy);

            _ = services.AddHttpClient<IRouterConfigManager, RouterConfigManager>(client => client.BaseAddress = new Uri(configuration.LoRaRegionRouterConfigUrl)).AddPolicyHandler(transientHttpErrorPolicy);

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
                  opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "AzureIoTHub.Portal.Server.xml"));
                  opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "AzureIoTHub.Portal.Shared.xml"));

                  opts.TagActionsBy(api => new[] { api.GroupName });
                  opts.DocInclusionPredicate((_, _) => true);

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

            _ = services.AddHealthChecks()
                .AddCheck<IoTHubHealthCheck>("iothubHealth")
                .AddCheck<StorageAccountHealthCheck>("storageAccountHealth")
                .AddCheck<TableStorageHealthCheck>("tableStorageHealth")
                .AddCheck<ProvisioningServiceClientHealthCheck>("dpsHealth")
                .AddCheck<LoRaManagementKeyFacadeHealthCheck>("loraManagementFacadeHealth");
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ArgumentNullException.ThrowIfNull(env, nameof(env));
            ArgumentNullException.ThrowIfNull(app, nameof(app));

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
                _ = endpoints.Map("api/{**slug}", HandleApiFallback);

                // If this is a request for a web page, just do the normal out-of-the-box behaviour.
                _ = endpoints.MapFallbackToFile("{**slug}", "index.html", new StaticFileOptions
                {
                    OnPrepareResponse = ctx => ctx.Context.Response.Headers.Add("Cache-Control", new StringValues("no-cache"))
                });

                _ = endpoints.MapHealthChecks("/healthz");
            });

            await app?.ApplicationServices
                    .GetService<IDeviceModelImageManager>()
                    .InitializeDefaultImageBlob();
        }

        private Task HandleApiFallback(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }
    }
}
