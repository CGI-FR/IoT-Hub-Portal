// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Postgres
{
    using AzureIoTHub.Portal.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class PortalDbContextFactory : IDesignTimeDbContextFactory<PortalDbContext>
    {
        public PortalDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();

            var connectionString = "Server=database;Database=cgigeiotdemo;Port=5432;User Id=postgres;Password=postgrePassword;Pooling=true;Connection Lifetime=0;Command Timeout=0;";
            _ = optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsAssembly("AzureIoTHub.Portal.Postgres"));

            return new PortalDbContext(optionsBuilder.Options);
        }
    }
}
