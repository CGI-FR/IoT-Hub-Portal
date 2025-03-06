// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Startup
{
    public static class AWSServiceCollectionExtension
    {
        public static IServiceCollection AddAWSInfrastructureLayer(this IServiceCollection services, ConfigHandler configuration)
        {
            return services
                .ConfigureAWSClient(configuration).Result
                .ConfigureAWSServices()
                .ConfigureAWSDeviceModelImages()
                .ConfigureAWSSyncJobs(configuration)
                .ConfigureAWSSendingCommands(configuration);
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
                .AddTransient<IExternalDeviceService, AwsExternalDeviceService>()
                .AddTransient(typeof(IDeviceModelService<,>), typeof(AwsDeviceModelService<,>))
                .AddTransient<IDeviceService<DeviceDetails>, AWSDeviceService>()
                .AddTransient<IDevicePropertyService, AWSDevicePropertyService>()
                .AddTransient<IConfigService, AwsConfigService>()
                .AddTransient<IEdgeModelService, EdgeModelService>()
                .AddTransient<IEdgeDevicesService, AWSEdgeDevicesService>();

        }

        private static IServiceCollection ConfigureAWSDeviceModelImages(this IServiceCollection services)
        {
            _ = services.AddTransient<IDeviceModelImageManager, AwsDeviceModelImageManager>();

            return services;
        }

        private static IServiceCollection ConfigureAWSSyncJobs(this IServiceCollection services, ConfigHandler configuration)
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

                _ = q.AddJob<DeviceMetricLoaderJob>(j => j.WithIdentity(nameof(DeviceMetricLoaderJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(DeviceMetricLoaderJob)}")
                        .ForJob(nameof(DeviceMetricLoaderJob))
                    .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));

                _ = q.AddJob<DeviceMetricExporterJob>(j => j.WithIdentity(nameof(DeviceMetricExporterJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(DeviceMetricExporterJob)}")
                        .ForJob(nameof(DeviceMetricExporterJob))
                    .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));

                _ = q.AddJob<EdgeDeviceMetricLoaderJob>(j => j.WithIdentity(nameof(EdgeDeviceMetricLoaderJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(EdgeDeviceMetricLoaderJob)}")
                        .ForJob(nameof(EdgeDeviceMetricLoaderJob))
                    .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SyncDatabaseJobRefreshIntervalInMinutes)
                            .RepeatForever()));
            });
        }

        private static IServiceCollection ConfigureAWSSendingCommands(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.AddQuartz(q =>
            {
                _ = q.AddJob<SendPlanningCommandJob>(j => j.WithIdentity(nameof(SendPlanningCommandJob)))
                    .AddTrigger(t => t
                        .WithIdentity($"{nameof(SendPlanningCommandJob)}")
                        .ForJob(nameof(SendPlanningCommandJob))
                        .WithSimpleSchedule(s => s
                            .WithIntervalInMinutes(configuration.SendCommandsToDevicesIntervalInMinutes)
                            .RepeatForever()));
            });
        }

    }
}
