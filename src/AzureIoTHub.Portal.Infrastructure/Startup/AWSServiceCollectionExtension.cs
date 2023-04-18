// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Startup
{
    using Amazon;
    using Amazon.IoT;
    using Amazon.IotData;
    using Amazon.S3;
    using Amazon.SecretsManager;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Infrastructure.Managers;
    using Microsoft.Extensions.DependencyInjection;

    public static class AWSServiceCollectionExtension
    {
        public static IServiceCollection AddAWSInfrastructureLayer(this IServiceCollection services, ConfigHandler configuration)
        {
            return services.ConfigureAWSClient(configuration);
        }
        private static IServiceCollection ConfigureAWSClient(this IServiceCollection services, ConfigHandler configuration)
        {
            _ = services.AddSingleton(() => new AmazonIoTClient(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));
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

            _ = services.AddSingleton(() => new AmazonSecretsManagerClient(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));

            _ = services.AddSingleton(() => new AmazonS3Client(configuration.AWSAccess, configuration.AWSAccessSecret, RegionEndpoint.GetBySystemName(configuration.AWSRegion)));

            _ = services.AddTransient<IDeviceModelImageManager, AwsDeviceModelImageManager>();

            return services;
        }
    }
}
