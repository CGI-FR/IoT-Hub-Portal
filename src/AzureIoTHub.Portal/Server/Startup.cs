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
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Infrastructure.Factories;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Seeds;
    using Extensions;
    using Hellang.Middleware.ProblemDetails;
    using Hellang.Middleware.ProblemDetails.Mvc;
    using Identity;
    using Managers;
    using Mappers;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.OpenApi.Models;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using MudBlazor.Services;
    using Polly;
    using Polly.Extensions.Http;
    using Prometheus;
    using Quartz;
    using Services;
    using ServicesHealthCheck;
    using Shared.Models.v1._0;
    using Wrappers;

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

            var configuration = ConfigHandlerFactory.Create(HostEnvironment, Configuration);

            _ = services.Configure<ClientApiIndentityOptions>(opts =>
            {
                opts.MetadataUrl = new Uri(configuration.OIDCMetadataUrl);
                opts.ClientId = configuration.OIDCClientId;
                opts.Scope = configuration.OIDCScope;
                opts.Authority = configuration.OIDCAuthority;
            });

            _ = services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opts =>
                {
                    opts.Authority = configuration.OIDCAuthority;
                    opts.MetadataAddress = configuration.OIDCMetadataUrl;
                    opts.Audience = configuration.OIDCApiClientId;

                    opts.TokenValidationParameters.ValidateIssuer = configuration.OIDCValidateIssuer;
                    opts.TokenValidationParameters.ValidateAudience = configuration.OIDCValidateAudience;
                    opts.TokenValidationParameters.ValidateLifetime = configuration.OIDCValidateLifetime;
                    opts.TokenValidationParameters.ValidateIssuerSigningKey = configuration.OIDCValidateIssuerSigningKey;
                    opts.TokenValidationParameters.ValidateActor = configuration.OIDCValidateActor;
                    opts.TokenValidationParameters.ValidateTokenReplay = configuration.OIDCValidateTokenReplay;
                });

            _ = services
                .AddDbContextPool<PortalDbContext>(opts => opts.UseNpgsql(configuration.PostgreSQLConnectionString));

            _ = services.AddScoped<IUnitOfWork, UnitOfWork<PortalDbContext>>();

            _ = services.AddSingleton(configuration);
            _ = services.AddSingleton(new PortalMetric());

            _ = services.AddRazorPages();

            _ = services.AddScoped(_ => RegistryManager.CreateFromConnectionString(configuration.IoTHubConnectionString));

            _ = services.AddScoped(_ => ServiceClient.CreateFromConnectionString(configuration.IoTHubConnectionString));

            _ = services.AddScoped(_ => ProvisioningServiceClient.CreateFromConnectionString(configuration.DPSConnectionString));

            _ = services.AddTransient<IProvisioningServiceClient, ProvisioningServiceClientWrapper>();
            _ = services.AddTransient(_ => new BlobServiceClient(configuration.StorageAccountConnectionString));
            _ = services.AddTransient<ITableClientFactory>(_ => new TableClientFactory(configuration.StorageAccountConnectionString));
            _ = services.AddTransient<IDeviceModelImageManager, DeviceModelImageManager>();
            _ = services.AddTransient<IConcentratorTwinMapper, ConcentratorTwinMapper>();
            _ = services.AddTransient<IDeviceModelCommandMapper, DeviceModelCommandMapper>();
            _ = services.AddTransient<IDeviceModelCommandsManager, DeviceModelCommandsManager>();
            _ = services.AddTransient<IDeviceProvisioningServiceManager, DeviceProvisioningServiceManager>();
            _ = services.AddTransient<IRouterConfigManager, RouterConfigManager>();

            _ = services.AddTransient<IDeviceTwinMapper<DeviceListItem, DeviceDetails>, DeviceTwinMapper>();
            _ = services.AddTransient<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>, LoRaDeviceTwinMapper>();
            _ = services.AddTransient<IDeviceModelMapper<DeviceModel, DeviceModel>, DeviceModelMapper>();
            _ = services.AddTransient<IDeviceModelMapper<DeviceModel, LoRaDeviceModel>, LoRaDeviceModelMapper>();
            _ = services.AddTransient<IDeviceTagMapper, DeviceTagMapper>();
            _ = services.AddTransient<IEdgeDeviceModelMapper, EdgeModelMapper>();
            _ = services.AddTransient<IEdgeDeviceMapper, EdgeDeviceMapper>();

            _ = services.AddTransient<IDeviceService, DeviceService>();
            _ = services.AddTransient<IConfigService, ConfigService>();
            _ = services.AddTransient<IDeviceTagService, DeviceTagService>();
            _ = services.AddTransient<ILoRaWANCommandService, LoRaWANCommandService>();
            _ = services.AddTransient<ILoRaWANDeviceService, LoRaWANDeviceService>();
            _ = services.AddTransient<IEdgeModelService, EdgeModelService>();
            _ = services.AddTransient<ILoRaWANConcentratorService, LoRaWANConcentratorService>();
            _ = services.AddTransient<IEdgeDevicesService, EdgeDevicesService>();
            _ = services.AddTransient<IDevicePropertyService, DevicePropertyService>();
            _ = services.AddTransient<IDeviceConfigurationsService, DeviceConfigurationsService>();
            _ = services.AddTransient<IDeviceModelPropertiesService, DeviceModelPropertiesService>();

            _ = services.AddScoped<IDeviceModelPropertiesRepository, DeviceModelPropertiesRepository>();

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
                client.DefaultRequestHeaders.Add("api-version", configuration.LoRaKeyManagementApiVersion ?? "2022-03-04");
            })
                .AddPolicyHandler(transientHttpErrorPolicy);

            ConfigureIdeasFeature(services, configuration);

            // Add problem details support
            _ = services.AddProblemDetails(setup =>
            {
                setup.IncludeExceptionDetails = (_, _) => HostEnvironment.IsDevelopment();

                // Custom mapping function for FluentValidation's ValidationException.
                setup.MapFluentValidationException();

                setup.Map<InternalServerErrorException>(exception => new ProblemDetails
                {
                    Title = exception.Title,
                    Detail = exception.Detail,
                    Status = StatusCodes.Status500InternalServerError
                });

                setup.Map<BaseException>(exception => new ProblemDetails
                {
                    Title = exception.Title,
                    Detail = exception.Detail,
                    Status = StatusCodes.Status400BadRequest
                });

                setup.Map<ArgumentNullException>(exception => new ProblemDetails
                {
                    Title = "Null Argument",
                    Detail = exception.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Extensions =
                    {
                        ["params"] = exception.ParamName
                    }
                });
            });

            _ = services.AddControllers();
            _ = services.AddProblemDetailsConventions();

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

                var securityDefinition = new OpenApiSecurityScheme
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

                var securityRequirements = new OpenApiSecurityRequirement
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
                .AddDbContextCheck<PortalDbContext>()
                .AddCheck<IoTHubHealthCheck>("iothubHealth")
                .AddCheck<StorageAccountHealthCheck>("storageAccountHealth")
                .AddCheck<TableStorageHealthCheck>("tableStorageHealth")
                .AddCheck<ProvisioningServiceClientHealthCheck>("dpsHealth")
                .AddCheck<LoRaManagementKeyFacadeHealthCheck>("loraManagementFacadeHealth");

            // Register metric loaders as Hosted Services
            _ = services.AddHostedService<DeviceMetricLoaderService>();
            _ = services.AddHostedService<EdgeDeviceMetricLoaderService>();
            _ = services.AddHostedService<ConcentratorMetricLoaderService>();

            // Register metric exporters as Hosted Services
            _ = services.AddHostedService<DeviceMetricExporterService>();
            _ = services.AddHostedService<EdgeDeviceMetricExporterService>();
            _ = services.AddHostedService<ConcentratorMetricExporterService>();

            // Add the required Quartz.NET services
            _ = services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.InterruptJobsOnShutdownWithWait = true;

                q.UsePersistentStore(opts =>
                {
                    // JSON is recommended persistent format to store data in database for greenfield projects.
                    // You should also strongly consider setting useProperties to true to restrict key - values to be strings.
                    opts.UseJsonSerializer();
                    opts.UseClustering();
                    opts.UseProperties = true;

                    opts.UsePostgres(c =>
                    {
                        c.ConnectionString = configuration.PostgreSQLConnectionString;
                    });
                });
            });

            // Add the Quartz.NET hosted service
            _ = services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }

        private static void ConfigureIdeasFeature(IServiceCollection services, ConfigHandler configuration)
        {
            _ = services.AddTransient<IIdeaService, IdeaService>();

            _ = services.AddHttpClient<IIdeaService, IdeaService>(client =>
            {
                if (!configuration.IdeasEnabled) return;
                client.BaseAddress = new Uri(configuration.IdeasUrl);
                client.DefaultRequestHeaders.Add(configuration.IdeasAuthenticationHeader,
                    configuration.IdeasAuthenticationToken);
            });
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

            var configuration = app.ApplicationServices.GetService<ConfigHandler>();

            // Use problem details
            _ = app.UseProblemDetails();
            app.UseIfElse(IsApiRequest, UseApiExceptionMiddleware, UseUIExceptionMiddleware);

            if (configuration.UseSecurityHeaders)
            {
                _ = app.UseSecurityHeaders(opts =>
                {
                    _ = opts.AddContentSecurityPolicy(csp =>
                    {
                        _ = csp.AddFrameAncestors()
                            .Self()
                            .From(configuration.OIDCMetadataUrl);
                    });
                });
            }

            if (env.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                _ = app.UseSwagger();
                _ = app.UseSwaggerUI(options =>
                {
                    // Disable swagger "Try It Out" feature
                    options.SupportedSubmitMethods();
                });
            }
            else
            {
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

                _ = endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteHealthReport
                });
            });

            var deviceModelImageManager = app.ApplicationServices.GetService<IDeviceModelImageManager>();

            await deviceModelImageManager?.InitializeDefaultImageBlob()!;
            await deviceModelImageManager?.SyncImagesCacheControl()!;

            await EnsureDatabaseCreatedAndUpToDate(app)!;
        }

        private static void UseApiExceptionMiddleware(IApplicationBuilder app)
        {
            _ = app.UseProblemDetails();
        }

        private void UseUIExceptionMiddleware(IApplicationBuilder app)
        {
            _ = HostEnvironment.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseExceptionHandler("/Error");
        }

        private static bool IsApiRequest(HttpContext httpContext)
        {
            return httpContext.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
        }

        private Task HandleApiFallback(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }

        private static async Task EnsureDatabaseCreatedAndUpToDate(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<ConfigHandler>();

            await context.Database.MigrateAsync();

            try
            {
                await context
                    .MigrateDeviceModelProperties(config);

                _ = await context.SaveChangesAsync();
            }
            catch (InvalidOperationException e)
            {
                scope.ServiceProvider.GetRequiredService<ILogger>().LogError(e, "Failed to seed the database.");
            }
        }
    }
}
