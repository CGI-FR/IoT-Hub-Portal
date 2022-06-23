// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Shared.Models.v1._0;

    public class ConcentratorMetricLoaderService : BackgroundService
    {
        private readonly ILogger<ConcentratorMetricLoaderService> logger;
        private readonly ConfigHandler configHandler;
        private readonly PortalMetric portalMetric;

        public ConcentratorMetricLoaderService(ILogger<ConcentratorMetricLoaderService> logger, ConfigHandler configHandler, PortalMetric portalMetric, IServiceProvider services)
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
                this.logger.LogInformation("Start loading concentrators metrics");

                using var scope = Services.CreateScope();

                await LoadConcentratorsCountMetric(scope);
                await LoadConnectedConcentratorsCountMetric(scope);

                this.logger.LogInformation("End loading concentrators metrics");
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task LoadConcentratorsCountMetric(IServiceScope scope)
        {
            try
            {
                var deviceService =
                    scope.ServiceProvider
                        .GetRequiredService<IDeviceService>();

                this.portalMetric.ConcentratorCount = await deviceService.GetConcentratorsCount();

            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load concentrators count metric: {e.Detail}", e);
            }
        }

        private async Task LoadConnectedConcentratorsCountMetric(IServiceScope scope)
        {
            try
            {
                var deviceService =
                    scope.ServiceProvider
                        .GetRequiredService<IDeviceService>();

                this.portalMetric.ConnectedConcentratorCount = await deviceService.GetConnectedConcentratorsCount();

            }
            catch (InternalServerErrorException e)
            {
                this.logger.LogError($"Unable to load connected concentrators count metric: {e.Detail}", e);
            }
        }
    }
}
