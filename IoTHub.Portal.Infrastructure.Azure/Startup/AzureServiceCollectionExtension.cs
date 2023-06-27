// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Startup
{
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Application.Wrappers;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Options;
    using IoTHub.Portal.Infrastructure.Azure.Jobs;
    using IoTHub.Portal.Infrastructure.Azure.Managers;
    using IoTHub.Portal.Infrastructure.Azure.Mappers;
    using IoTHub.Portal.Infrastructure.Azure.Providers;
    using IoTHub.Portal.Infrastructure.Azure.Services;
    using IoTHub.Portal.Infrastructure.Azure.ServicesHealthCheck;
    using IoTHub.Portal.Infrastructure.Azure.Wrappers;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.Extensions.Http;
    using Quartz;
    using System.Net;
    using System.Reflection;

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

            var transientHttpErrorPolicy = HttpPolicyExtensions
                                    .HandleTransientHttpError()
                                    .OrResult(c => c.StatusCode == HttpStatusCode.NotFound)
                                    .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100));

            _ = services.AddHttpClient("RestClient")
                .AddPolicyHandler(transientHttpErrorPolicy);

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
            return services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }

        private static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            return services.AddTransient<ILoRaWanManagementService, LoRaWanManagementService>()
                .AddTransient<IDeviceService<DeviceDetailsDto>, DeviceService>()
                .AddTransient<IDeviceService<LoRaDeviceDetailsDto>, LoRaWanDeviceService>()
                            .AddTransient<IDeviceModelImageManager, DeviceModelImageManager>();
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
