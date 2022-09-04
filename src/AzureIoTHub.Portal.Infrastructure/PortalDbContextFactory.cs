// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class PortalDbContextFactory : IDesignTimeDbContextFactory<PortalDbContext>
    {
        public PortalDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();

            _ = optionsBuilder.UseNpgsql("Server=database;Database=cgigeiotdemo;Port=5432;User Id=postgres;Password=postgrePassword;Pooling=true;Connection Lifetime=0;Command Timeout=0;");

            return new PortalDbContext(optionsBuilder.Options);
        }
    }
}
