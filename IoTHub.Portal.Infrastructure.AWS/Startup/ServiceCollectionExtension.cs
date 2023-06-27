// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.AWS.Startup
{
    using Amazon;
    using Amazon.GreengrassV2;
    using Amazon.IoT;
    using Amazon.IotData;
    using Amazon.S3;
    using Amazon.SecretsManager;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Infrastructure.AWS.Managers;
    using IoTHub.Portal.Infrastructure.AWS.Services;
    using IoTHub.Portal.Infrastructure.Common.Services;
    using IoTHub.Portal.Infrastructure.Jobs.AWS;
    using IoTHub.Portal.Models.v10;
    using Microsoft.Extensions.DependencyInjection;
    using Quartz;
    using System.Reflection;
    using DeviceService = Services.DeviceService;

    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAWSInfrastructureLayer(this IServiceCollection services, ConfigHandler configuration)
        {
            return services
                .ConfigureAWSClient(configuration).Result
                .ConfigureAWSServices()
                .ConfigureAWSDeviceModelImages()
                .ConfigureMappers()
                .ConfigureSyncJobs(configuration);
        }
        private static async Task<IServiceCollection> ConfigureAWSClient(this IServiceCollection services, ConfigHandler configuration)
        {
            var awsIoTClient = new AmazonIoTClient(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion));
            _ = services.AddSingleton<IAmazonIoT>(awsIoTClient);

            var endPoint = await awsIoTClient.DescribeEndpointAsync(new Amazon.IoT.Model.DescribeEndpointRequest
            {
                EndpointType = "iot:Data-ATS"
            });

            _ = services.AddSingleton<IAmazonIotData>(new AmazonIotDataClient(configuration.AWSAccess, configuration.AWSAccessSecret, new AmazonIotDataConfig
            {
                ServiceURL = $"https://{endPoint.EndpointAddress}"
            }));

            _ = services.AddSingleton<IAmazonSecretsManager>(new AmazonSecretsManagerClient(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));

            _ = services.AddSingleton<IAmazonS3>(new AmazonS3Client(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));
            _ = services.AddSingleton<IAmazonGreengrassV2>(new AmazonGreengrassV2Client(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));

            return services;
        }

        private static IServiceCollection ConfigureAWSServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IExternalDeviceService, ExternalDeviceService>()
                .AddTransient(typeof(IDeviceModelService<,>), typeof(DeviceModelService<,>))
                .AddTransient<IDeviceService<DeviceDetailsDto>, DeviceService>()
                .AddTransient<IDevicePropertyService, DevicePropertyService>()
                .AddTransient<IConfigService, ConfigService>()
                .AddTransient<IEdgeModelService, EdgeModelService>()
                .AddTransient<IEdgeDevicesService, EdgeDevicesService>();
        }

        private static IServiceCollection ConfigureMappers(this IServiceCollection services)
        {
            return services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }

        private static IServiceCollection ConfigureAWSDeviceModelImages(this IServiceCollection services)
        {
            _ = services.AddTransient<IDeviceModelImageManager, AwsDeviceModelImageManager>();

            return services;
        }

        private static IServiceCollection ConfigureSyncJobs(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.AddQuartz(q =>
            {
                _ = q.AddJob<SyncThingTypesJob>(j => j.WithIdentity(nameof(SyncThingTypesJob)))
                .AddTrigger(t => t
                    .WithIdentity($"{nameof(SyncThingTypesJob)}")
                    .ForJob(nameof(SyncThingTypesJob))
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                        .RepeatForever()));

                _ = q.AddJob<SyncThingsJob>(j => j.WithIdentity(nameof(SyncThingsJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SyncThingsJob)}")
                        .ForJob(nameof(SyncThingsJob))
                        .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));

                _ = q.AddJob<SyncGreenGrassDeploymentsJob>(j => j.WithIdentity(nameof(SyncGreenGrassDeploymentsJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SyncGreenGrassDeploymentsJob)}")
                        .ForJob(nameof(SyncGreenGrassDeploymentsJob))
                    .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));
            });
        }
    }
}
