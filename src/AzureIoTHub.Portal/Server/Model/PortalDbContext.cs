// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Model
{
    using Microsoft.EntityFrameworkCore;

    public class PortalDbContext : DbContext
    {
        public DbSet<DeviceModelProperty> DeviceModelProperties { get; set; }

        public PortalDbContext(DbContextOptions<PortalDbContext> options)
            : base(options)
        { }
    }
}
