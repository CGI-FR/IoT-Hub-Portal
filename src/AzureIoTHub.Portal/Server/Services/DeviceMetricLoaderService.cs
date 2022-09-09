// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Shared.Models.v1._0;

    public class DeviceMetricLoaderService : BackgroundService
    {
        private readonly ILogger<DeviceMetricLoaderService> logger;
        private readonly ConfigHandler configHandler;
        private readonly PortalMetric portalMetric;

        public DeviceMetricLoaderService(ILogger<DeviceMetricLoaderService> logger, ConfigHandler configHandler, PortalMetric portalMetric, IServiceProvider services)
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
                this.logger.LogInformation("Start loading devices metrics");

                using var scope = Services.CreateScope();

                await LoadDevicesCountMetric(scope);
                await LoadConnectedDevicesCountMetric(scope);

                this.logger.LogInformation("End loading devices metrics");
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task LoadDevicesCountMetric(IServiceScope scope)
        {
            try
            {
                var deviceService =
                    scope.ServiceProvider
                        .GetRequiredService<IDeviceService>();

                this.portalMetric.DeviceCount = await deviceService.GetDevicesCount();

            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load devices count metric: {e.Detail}", e);
            }
        }

        private async Task LoadConnectedDevicesCountMetric(IServiceScope scope)
        {
            try
            {
                var deviceService =
                    scope.ServiceProvider
                        .GetRequiredService<IDeviceService>();

                this.portalMetric.ConnectedDeviceCount = await deviceService.GetConnectedDevicesCount();

            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load connected devices count metric: {e.Detail}", e);
            }
        }
    }
}
