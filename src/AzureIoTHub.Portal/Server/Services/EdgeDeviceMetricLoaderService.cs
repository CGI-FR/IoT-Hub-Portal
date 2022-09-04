// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain;
    using Exceptions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Shared.Models.v1._0;

    public class EdgeDeviceMetricLoaderService : BackgroundService
    {
        private readonly ILogger<EdgeDeviceMetricLoaderService> logger;
        private readonly ConfigHandler configHandler;
        private readonly PortalMetric portalMetric;

        public EdgeDeviceMetricLoaderService(ILogger<EdgeDeviceMetricLoaderService> logger, ConfigHandler configHandler, PortalMetric portalMetric, IServiceProvider services)
        {
            this.logger = logger;
            this.configHandler = configHandler;
            this.portalMetric = portalMetric;
            Services = services;
        }

        public IServiceProvider Services { get; }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(this.configHandler.MetricLoaderRefreshIntervalInMinutes));

            do
            {
                this.logger.LogInformation("Start loading edge devices metrics");

                using var scope = Services.CreateScope();

                await LoadEdgeDevicesCountMetric(scope);
                await LoadConnectedEdgeDevicesCountMetric(scope);
                await LoadFailedDeploymentsCountMetric(scope);

                this.logger.LogInformation("End loading edge devices metrics");
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task LoadEdgeDevicesCountMetric(IServiceScope scope)
        {
            try
            {
                var deviceService =
                    scope.ServiceProvider
                        .GetRequiredService<IDeviceService>();

                this.portalMetric.EdgeDeviceCount = await deviceService.GetEdgeDevicesCount();

            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load edge devices count metric: {e.Detail}", e);
            }
        }

        private async Task LoadConnectedEdgeDevicesCountMetric(IServiceScope scope)
        {
            try
            {
                var deviceService =
                    scope.ServiceProvider
                        .GetRequiredService<IDeviceService>();

                this.portalMetric.ConnectedEdgeDeviceCount = await deviceService.GetConnectedEdgeDevicesCount();

            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load connected edge devices count metric: {e.Detail}", e);
            }
        }

        private async Task LoadFailedDeploymentsCountMetric(IServiceScope scope)
        {
            try
            {
                var configService =
                    scope.ServiceProvider
                        .GetRequiredService<IConfigService>();

                this.portalMetric.FailedDeploymentCount = await configService.GetFailedDeploymentsCount();

            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load failed deployments count metric: {e.Detail}", e);
            }
        }
    }
}
