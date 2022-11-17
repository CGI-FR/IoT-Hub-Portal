// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories
{
    using System.Linq.Dynamic.Core;
    using Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;

    public class DeviceTelemetryRepository : GenericRepository<DeviceTelemetry>, IDeviceTelemetryRepository
    {
        public DeviceTelemetryRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
