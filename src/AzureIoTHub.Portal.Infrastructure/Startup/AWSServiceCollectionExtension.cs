// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Startup
{
    using Amazon;
    using Amazon.GreengrassV2;
    using Amazon.IoT;
    using Amazon.IotData;
    using Amazon.S3;
    using Amazon.SecretsManager;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Infrastructure.Jobs.AWS;
    using AzureIoTHub.Portal.Infrastructure.Managers;
    using AzureIoTHub.Portal.Infrastructure.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Quartz;

    public static class AWSServiceCollectionExtension
    {
        public static IServiceCollection AddAWSInfrastructureLayer(this IServiceCollection services, ConfigHandler configuration)
        {
            return services
                .ConfigureAWSClient(configuration)
                .ConfigureAWSServices()
                .ConfigureAWSDeviceModelImages()
                .ConfigureAWSSyncJobs(configuration);
        }
        private static IServiceCollection ConfigureAWSClient(this IServiceCollection services, ConfigHandler configuration)
        {
            _ = services.AddSingleton<IAmazonIoT>(new AmazonIoTClient(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));
            _ = services.AddSingleton(async sp =>
            {
                var endpoint = await sp.GetService<AmazonIoTClient>()!.DescribeEndpointAsync(new Amazon.IoT.Model.DescribeEndpointRequest
                {
                    EndpointType = "iot:Data-ATS"
                });

                return new AmazonIotDataClient(configuration.AWSAccess, configuration.AWSAccessSecret, new AmazonIotDataConfig
                {
                    ServiceURL = $"https://{endpoint.EndpointAddress}"
                });
            });

            _ = services.AddSingleton<IAmazonSecretsManager>(new AmazonSecretsManagerClient(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));

            _ = services.AddSingleton<IAmazonS3>(new AmazonS3Client(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));
            _ = services.AddSingleton<IAmazonGreengrassV2>(new AmazonGreengrassV2Client(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));

            return services;
        }

        private static IServiceCollection ConfigureAWSServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IExternalDeviceService, AwsExternalDeviceService>()
                .AddTransient(typeof(IDeviceModelService<,>), typeof(AwsDeviceModelService<,>));
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
            });
        }
    }
}
