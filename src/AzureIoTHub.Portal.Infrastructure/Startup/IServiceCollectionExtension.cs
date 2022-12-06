// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Startup
{
    using System.Net;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Mappers;
    using AzureIoTHub.Portal.Application.Providers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Application.Wrappers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Options;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Extensions;
    using AzureIoTHub.Portal.Infrastructure.Jobs;
    using AzureIoTHub.Portal.Infrastructure.Managers;
    using AzureIoTHub.Portal.Infrastructure.Mappers;
    using AzureIoTHub.Portal.Infrastructure.Providers;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Services;
    using AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck;
    using AzureIoTHub.Portal.Infrastructure.Wrappers;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using EntityFramework.Exceptions.PostgreSQL;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.Extensions.Http;
    using Quartz;

    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.ConfigureDatabase(configuration)
                            .ConfigureRepositories()
                            .ConfigureImageBlobStorage(configuration)
                            .AddLoRaWanSupport(configuration)
                            .ConfigureDeviceRegstryDependencies(configuration)
                            .ConfigureServices()
                            .ConfigureMappers()
                            .ConfigureHealthCheck()
                            .ConfigureMetricsJobs(configuration)
                            .ConfigureSyncJobs(configuration);
        }

        private static IServiceCollection AddLoRaWanSupport(this IServiceCollection services, ConfigHandler configuration)
        {
            _ = services.Configure<LoRaWANOptions>(opts =>
            {
                opts.Enabled = configuration.IsLoRaEnabled;
                opts.KeyManagementApiVersion = configuration.LoRaKeyManagementApiVersion;
                opts.KeyManagementCode = configuration.LoRaKeyManagementCode;
                opts.KeyManagementUrl = configuration.LoRaKeyManagementUrl;
            });

            if (!configuration.IsLoRaEnabled)
            {
                return services;
            }

            var transientHttpErrorPolicy = HttpPolicyExtensions
                                    .HandleTransientHttpError()
                                    .OrResult(c => c.StatusCode == HttpStatusCode.NotFound)
                                    .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100));

            _ = services.AddHttpClient("RestClient")
                .AddPolicyHandler(transientHttpErrorPolicy);

            _ = services.AddHttpClient<ILoRaWanManagementService, LoRaWanManagementService>((sp, client) =>
            {
                var opts = sp.GetService<IOptions<LoRaWANOptions>>().Value;

                client.BaseAddress = new Uri(opts.KeyManagementUrl);
                client.DefaultRequestHeaders.Add("x-functions-key", opts.KeyManagementCode);
                client.DefaultRequestHeaders.Add("api-version", opts.KeyManagementApiVersion ?? "2022-03-04");
            })
                .AddPolicyHandler(transientHttpErrorPolicy);

            return services;
        }

        private static IServiceCollection ConfigureDeviceRegstryDependencies(this IServiceCollection services, ConfigHandler configuration)
        {
            _ = services.AddTransient<IProvisioningServiceClient, ProvisioningServiceClientWrapper>();
            _ = services.AddTransient<IDeviceRegistryProvider, AzureDeviceRegistryProvider>();

            _ = services.AddScoped(_ => RegistryManager.CreateFromConnectionString(configuration.IoTHubConnectionString));
            _ = services.AddScoped(_ => ServiceClient.CreateFromConnectionString(configuration.IoTHubConnectionString));
            _ = services.AddScoped(_ => ProvisioningServiceClient.CreateFromConnectionString(configuration.DPSConnectionString));

            return services;
        }

        private static IServiceCollection ConfigureDatabase(this IServiceCollection services, ConfigHandler configuration)
        {
            _ = services
                .AddDbContextPool<PortalDbContext>(opts =>
                {
                    _ = opts.UseNpgsql(configuration.PostgreSQLConnectionString);
                    _ = opts.UseExceptionProcessor();
                });

            if (string.IsNullOrEmpty(configuration.PostgreSQLConnectionString))
                return services;

            _ = services.AddScoped<IUnitOfWork, UnitOfWork<PortalDbContext>>();

            var dbContextOptions = new DbContextOptionsBuilder<PortalDbContext>();
            _ = dbContextOptions.UseNpgsql(configuration.PostgreSQLConnectionString);

            using var ctx = new PortalDbContext(dbContextOptions.Options);
            ctx.Database.Migrate();

            return services;
        }

        private static IServiceCollection ConfigureRepositories(this IServiceCollection services)
        {
            return services.AddScoped<IDeviceModelPropertiesRepository, DeviceModelPropertiesRepository>()
                            .AddScoped<IDeviceTagRepository, DeviceTagRepository>()
                            .AddScoped<IEdgeDeviceModelRepository, EdgeDeviceModelRepository>()
                            .AddScoped<IEdgeDeviceModelCommandRepository, EdgeDeviceModelCommandRepository>()
                            .AddScoped<IDeviceModelRepository, DeviceModelRepository>()
                            .AddScoped<IDeviceRepository, DeviceRepository>()
                            .AddScoped<IEdgeDeviceRepository, EdgeDeviceRepository>()
                            .AddScoped<ILorawanDeviceRepository, LorawanDeviceRepository>()
                            .AddScoped<IDeviceTagValueRepository, DeviceTagValueRepository>()
                            .AddScoped<IDeviceModelCommandRepository, DeviceModelCommandRepository>()
                            .AddScoped<IConcentratorRepository, ConcentratorRepository>()
                            .AddScoped<ILoRaDeviceTelemetryRepository, LoRaDeviceTelemetryRepository>()
                            .AddScoped<ILabelRepository, LabelRepository>();
        }

        private static IServiceCollection ConfigureMappers(this IServiceCollection services)
        {
            _ = services.AddTransient<IDeviceModelImageManager, DeviceModelImageManager>();
            _ = services.AddTransient<IConcentratorTwinMapper, ConcentratorTwinMapper>();
            _ = services.AddTransient<IDeviceModelCommandMapper, DeviceModelCommandMapper>();

            return services.AddTransient<IDeviceTwinMapper<DeviceListItem, DeviceDetails>, DeviceTwinMapper>()
                            .AddTransient<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>, LoRaDeviceTwinMapper>()
                            .AddTransient<IDeviceModelMapper<DeviceModelDto, DeviceModelDto>, DeviceModelMapper>()
                            .AddTransient<IDeviceModelMapper<DeviceModelDto, LoRaDeviceModelDto>, LoRaDeviceModelMapper>()
                            .AddTransient<IEdgeDeviceMapper, EdgeDeviceMapper>();
        }

        private static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            return services.AddTransient<ILoRaWanManagementService, LoRaWanManagementService>();
        }

        private static IServiceCollection ConfigureHealthCheck(this IServiceCollection services)
        {
            _ = services.AddHealthChecks()
               .AddDbContextCheck<PortalDbContext>()
               .AddCheck<IoTHubHealthCheck>("iothubHealth")
               .AddCheck<StorageAccountHealthCheck>("storageAccountHealth")
               .AddCheck<TableStorageHealthCheck>("tableStorageHealth")
               .AddCheck<ProvisioningServiceClientHealthCheck>("dpsHealth")
               .AddCheck<LoRaManagementKeyFacadeHealthCheck>("loraManagementFacadeHealth")
               .AddCheck<DatabaseHealthCheck>("databaseHealthCheck");

            return services;
        }

        private static IServiceCollection ConfigureImageBlobStorage(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.AddTransient(_ => new BlobServiceClient(configuration.StorageAccountConnectionString))
                            .Configure<DeviceModelImageOptions>((opts) =>
                            {
                                var serviceClient = new BlobServiceClient(configuration.StorageAccountConnectionString);
                                var container = serviceClient.GetBlobContainerClient(opts.ImageContainerName);

                                _ = container.SetAccessPolicy(PublicAccessType.Blob);
                                _ = container.CreateIfNotExists();

                                opts.BaseUri = container.Uri;
                            });
        }

        private static IServiceCollection ConfigureMetricsJobs(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.AddQuartz(q =>
            {
                q.AddMetricsService<DeviceMetricExporterJob, DeviceMetricLoaderJob>(configuration);
                q.AddMetricsService<EdgeDeviceMetricExporterJob, EdgeDeviceMetricLoaderJob>(configuration);
                q.AddMetricsService<ConcentratorMetricExporterJob, ConcentratorMetricLoaderJob>(configuration);
            });
        }

        private static IServiceCollection ConfigureSyncJobs(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.AddQuartz(q =>
            {
                _ = q.AddJob<SyncDevicesJob>(j => j.WithIdentity(nameof(SyncDevicesJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SyncDevicesJob)}")
                        .ForJob(nameof(SyncDevicesJob))
                    .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));

                _ = q.AddJob<SyncConcentratorsJob>(j => j.WithIdentity(nameof(SyncConcentratorsJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SyncConcentratorsJob)}")
                        .ForJob(nameof(SyncConcentratorsJob))
                    .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));

                _ = q.AddJob<SyncEdgeDeviceJob>(j => j.WithIdentity(nameof(SyncEdgeDeviceJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SyncEdgeDeviceJob)}")
                        .ForJob(nameof(SyncEdgeDeviceJob))
                        .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));

                _ = q.AddJob<SyncGatewayIDJob>(j => j.WithIdentity(nameof(SyncGatewayIDJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SyncGatewayIDJob)}")
                        .ForJob(nameof(SyncGatewayIDJob))
                        .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));

                if (configuration.IsLoRaEnabled)
                {
                    _ = q.AddJob<SyncLoRaDeviceTelemetryJob>(j => j.WithIdentity(nameof(SyncLoRaDeviceTelemetryJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SyncLoRaDeviceTelemetryJob)}")
                        .ForJob(nameof(SyncLoRaDeviceTelemetryJob))
                        .StartNow());
                }
            });
        }
    }
}
