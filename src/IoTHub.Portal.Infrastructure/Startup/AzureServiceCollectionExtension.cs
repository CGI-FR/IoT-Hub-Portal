// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Startup
{
    using System.Net;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Extenstions;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Mappers;
    using IoTHub.Portal.Application.Providers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Application.Wrappers;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Options;
    using IoTHub.Portal.Infrastructure.Jobs;
    using IoTHub.Portal.Infrastructure.Managers;
    using IoTHub.Portal.Infrastructure.Mappers;
    using IoTHub.Portal.Infrastructure.Providers;
    using IoTHub.Portal.Infrastructure.Services;
    using IoTHub.Portal.Infrastructure.ServicesHealthCheck;
    using IoTHub.Portal.Infrastructure.Wrappers;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.Extensions.Http;
    using Quartz;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;

    public static class AzureServiceCollectionExtension
    {
        public static IServiceCollection AddAzureInfrastructureLayer(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.ConfigureImageBlobStorage(configuration)
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
                opts.KeyManagementApiVersion = configuration.AzureLoRaKeyManagementApiVersion;
                opts.KeyManagementCode = configuration.AzureLoRaKeyManagementCode;
                opts.KeyManagementUrl = configuration.AzureLoRaKeyManagementUrl;
            });

            if (!configuration.IsLoRaEnabled)
            {
                return services;
            }

            _ = services.AddTransient<ILoRaWanManagementService, LoRaWanManagementService>()
                        .AddTransient<IDeviceService<LoRaDeviceDetails>, LoRaWanDeviceService>();

            var transientHttpErrorPolicy = HttpPolicyExtensions
                                 .HandleTransientHttpError()
                                 .OrResult(c => c.StatusCode == HttpStatusCode.NotFound)
                                 .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100));

            _ = services.AddHttpClient<ILoRaWanManagementService, LoRaWanManagementService>((sp, client) =>
            {
                var opts = sp.GetService<IOptions<LoRaWANOptions>>()?.Value;

                client.BaseAddress = new Uri(opts?.KeyManagementUrl!);
                client.DefaultRequestHeaders.Add("x-functions-key", opts?.KeyManagementCode);
                client.DefaultRequestHeaders.Add("api-version", opts?.KeyManagementApiVersion ?? "2022-03-04");
            })
                .AddPolicyHandler(transientHttpErrorPolicy);

            return services;
        }

        private static IServiceCollection ConfigureDeviceRegstryDependencies(this IServiceCollection services, ConfigHandler configuration)
        {
            _ = services.AddTransient<IProvisioningServiceClient, ProvisioningServiceClientWrapper>();
            _ = services.AddTransient<IDeviceRegistryProvider, AzureDeviceRegistryProvider>();

            _ = services.AddScoped(_ => RegistryManager.CreateFromConnectionString(configuration.AzureIoTHubConnectionString));
            _ = services.AddScoped(_ => ServiceClient.CreateFromConnectionString(configuration.AzureIoTHubConnectionString));
            _ = services.AddScoped(_ => ProvisioningServiceClient.CreateFromConnectionString(configuration.AzureDPSConnectionString));

            return services;
        }

        private static IServiceCollection ConfigureMappers(this IServiceCollection services)
        {
            return services.AddTransient<IDeviceTwinMapper<DeviceListItem, DeviceDetails>, DeviceTwinMapper>()
                            .AddTransient<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>, LoRaDeviceTwinMapper>()
                            .AddTransient<IDeviceModelMapper<DeviceModelDto, DeviceModelDto>, DeviceModelMapper>()
                            .AddTransient<IDeviceModelMapper<DeviceModelDto, LoRaDeviceModelDto>, LoRaDeviceModelMapper>()
                            .AddTransient<IEdgeDeviceMapper, EdgeDeviceMapper>()
                            .AddTransient<IDeviceModelImageManager, DeviceModelImageManager>()
                            .AddTransient<IConcentratorTwinMapper, ConcentratorTwinMapper>()
                            .AddTransient<IDeviceModelCommandMapper, DeviceModelCommandMapper>();
        }

        private static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IDeviceService<DeviceDetails>, DeviceService>();
        }

        private static IServiceCollection ConfigureHealthCheck(this IServiceCollection services)
        {
            _ = services.AddHealthChecks()
               .AddCheck<IoTHubHealthCheck>("iothubHealth")
               .AddCheck<StorageAccountHealthCheck>("storageAccountHealth")
               .AddCheck<ProvisioningServiceClientHealthCheck>("dpsHealth")
               .AddCheck<LoRaManagementKeyFacadeHealthCheck>("loraManagementFacadeHealth");

            return services;
        }

        private static IServiceCollection ConfigureImageBlobStorage(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.AddTransient(_ => new BlobServiceClient(configuration.AzureStorageAccountConnectionString))
                            .Configure<DeviceModelImageOptions>((opts) =>
                            {
                                var serviceClient = new BlobServiceClient(configuration.AzureStorageAccountConnectionString);
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

                _ = q.AddJob<SyncEdgeDeviceJob>(j => j.WithIdentity(nameof(SyncEdgeDeviceJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SyncEdgeDeviceJob)}")
                        .ForJob(nameof(SyncEdgeDeviceJob))
                        .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));

                if (configuration.IsLoRaEnabled)
                {
                    _ = q.AddJob<SyncConcentratorsJob>(j => j.WithIdentity(nameof(SyncConcentratorsJob)))
                        .AddTrigger(t => t
                            .WithIdentity($"{nameof(SyncConcentratorsJob)}")
                            .ForJob(nameof(SyncConcentratorsJob))
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

                    _ = q.AddJob<SyncLoRaDeviceTelemetryJob>(j => j.WithIdentity(nameof(SyncLoRaDeviceTelemetryJob)))
                        .AddTrigger(t => t
                            .WithIdentity($"{nameof(SyncLoRaDeviceTelemetryJob)}")
                            .ForJob(nameof(SyncLoRaDeviceTelemetryJob))
                            .StartAt(DateTimeOffset.Now.AddMinutes(1)));
                }
            });
        }
    }
}
