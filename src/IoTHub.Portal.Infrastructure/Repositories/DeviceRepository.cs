// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using Device = Domain.Entities.Device;

    public class DeviceRepository : GenericRepository<Device>, IDeviceRepository
    {
        public DeviceRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
