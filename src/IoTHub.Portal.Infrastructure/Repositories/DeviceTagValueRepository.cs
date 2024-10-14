// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    public class DeviceTagValueRepository : GenericRepository<DeviceTagValue>, IDeviceTagValueRepository
    {
        public DeviceTagValueRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
