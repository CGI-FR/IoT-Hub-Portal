// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.MySql
{
    using IoTHub.Portal.Infrastructure.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class PortalDbContextFactory : IDesignTimeDbContextFactory<PortalDbContext>
    {
        public PortalDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();

            var connectionString = "server=localhost;database=cgigeiotdemo;user=root;password=pass";
            _ = optionsBuilder.UseMySql(connectionString, DatabaseHelper.GetMySqlServerVersion(connectionString), x => x.MigrationsAssembly("IoTHub.Portal.MySql"));

            return new PortalDbContext(optionsBuilder.Options);
        }
    }
}
