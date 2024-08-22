// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Extenstions
{
    using IoTHub.Portal.Domain;
    using Quartz;

    public static class QuartzConfiguratorExtension
    {
        public static void AddMetricsService<TExporterService, TLoaderService>(this IServiceCollectionQuartzConfigurator q, ConfigHandler configuration)
            where TExporterService : class, IJob
            where TLoaderService : class, IJob
        {
            _ = q.AddJob<TExporterService>(j => j.WithIdentity(typeof(TExporterService).Name))
                    .AddTrigger(t => t
                        .WithIdentity($"{typeof(TExporterService).Name}")
                        .ForJob(typeof(TExporterService).Name)
                        .WithSimpleSchedule(s => s
                            .WithIntervalInSeconds(configuration.MetricExporterRefreshIntervalInSeconds)
                            .RepeatForever()));

            _ = q.AddJob<TLoaderService>(j => j.WithIdentity(typeof(TLoaderService).Name))
                        .AddTrigger(t => t
                            .WithIdentity($"{typeof(TLoaderService).Name}")
                            .ForJob(typeof(TLoaderService).Name)
                            .WithSimpleSchedule(s => s
                                .WithIntervalInMinutes(configuration.MetricLoaderRefreshIntervalInMinutes)
                                .RepeatForever()));
        }
    }
}
