// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Startup
{
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Domain.Shared.Constants;
    using AzureIoTHub.Portal.Infrastructure.Helpers;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Services;
    using AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Constants;
    using EntityFramework.Exceptions.PostgreSQL;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Quartz;

    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, ConfigHandler configuration)
        {
            //Common configuration
            services = services.ConfigureDatabase(configuration)
                               .ConfigureHealthCheck()
                               .ConfigureRepositories()
                               .ConfigureServices();

            //CloudProvider-dependant configurations
            return configuration.CloudProvider switch
            {
                CloudProviders.Azure => services.AddAzureInfrastructureLayer(configuration),
                CloudProviders.AWS => services.AddAWSInfrastructureLayer(configuration),
                _ => services
            };
        }

        private static IServiceCollection ConfigureDatabase(this IServiceCollection services, ConfigHandler configuration)
        {
            var dbContextOptions = new DbContextOptionsBuilder<PortalDbContext>();

            switch (configuration.DbProvider)
            {
                case DbProviders.PostgreSQL:
                    if (string.IsNullOrEmpty(configuration.PostgreSQLConnectionString))
                    {
                        return services;
                    }
                    _ = services.AddDbContextPool<PortalDbContext>(opts =>
                    {
                        _ = opts.UseNpgsql(configuration.PostgreSQLConnectionString, x => x.MigrationsAssembly("AzureIoTHub.Portal.Postgres"));
                        _ = opts.UseExceptionProcessor();
                    });
                    _ = dbContextOptions.UseNpgsql(configuration.PostgreSQLConnectionString, x => x.MigrationsAssembly("AzureIoTHub.Portal.Postgres"));
                    break;
                case DbProviders.MySQL:
                    if (string.IsNullOrEmpty(configuration.MySQLConnectionString))
                    {
                        return services;
                    }
                    _ = services.AddDbContextPool<PortalDbContext>(opts =>
                    {
                        _ = opts.UseMySql(configuration.MySQLConnectionString, DatabaseHelper.GetMySqlServerVersion(configuration.MySQLConnectionString), x => x.MigrationsAssembly("AzureIoTHub.Portal.MySql"));
                        _ = opts.UseExceptionProcessor();
                    });
                    _ = dbContextOptions.UseMySql(configuration.MySQLConnectionString, DatabaseHelper.GetMySqlServerVersion(configuration.MySQLConnectionString), x => x.MigrationsAssembly("AzureIoTHub.Portal.MySql"));
                    break;
                default:
                    return services;
            }

            _ = services.AddScoped<IUnitOfWork, UnitOfWork<PortalDbContext>>();

            using var portalDbContext = new PortalDbContext(dbContextOptions.Options);
            if (portalDbContext.Database.CanConnect())
            {
                portalDbContext.Database.Migrate();
            }

            return services;
        }

        private static IServiceCollection ConfigureHealthCheck(this IServiceCollection services)
        {
            _ = services.AddHealthChecks()
               .AddDbContextCheck<PortalDbContext>()
               .AddCheck<DatabaseHealthCheck>("databaseHealthCheck");

            return services;
        }

        private static IServiceCollection ConfigureRepositories(this IServiceCollection services)
        {
            return services.AddScoped<IDeviceModelPropertiesRepository, DeviceModelPropertiesRepository>()
                            .AddScoped<IDeviceTagRepository, DeviceTagRepository>()
                            .AddScoped<IEdgeDeviceModelRepository, EdgeDeviceModelRepository>()
                            .AddScoped<IEdgeDeviceModelCommandRepository, EdgeDeviceModelCommandRepository>()
                            .AddScoped<IDeviceModelRepository, DeviceModelRepository>()
                            .AddScoped<IDeviceRepository, DeviceRepository>()
                            .AddScoped<IEdgeDeviceRepository, EdgeDeviceRepository>()
                            .AddScoped<ILorawanDeviceRepository, LorawanDeviceRepository>()
                            .AddScoped<IDeviceTagValueRepository, DeviceTagValueRepository>()
                            .AddScoped<IDeviceModelCommandRepository, DeviceModelCommandRepository>()
                            .AddScoped<IConcentratorRepository, ConcentratorRepository>()
                            .AddScoped<ILoRaDeviceTelemetryRepository, LoRaDeviceTelemetryRepository>()
                            .AddScoped<ILabelRepository, LabelRepository>();
        }

        private static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            return services
                .AddTransient<IEdgeEnrollementHelper, EdgeEnrollementHelper>()
                .AddTransient<IDeviceModelPropertiesService, DeviceModelPropertiesService>()
                .AddTransient<IDeviceTagService, DeviceTagService>()
                .AddTransient<IDeviceModelPropertiesService, DeviceModelPropertiesService>();
        }
    }
}
