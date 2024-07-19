// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;
    public class AccessControlRepository : GenericRepository<AccessControl>, IAccessControlRepository
    {
        public AccessControlRepository(PortalDbContext context) : base(context) { }

    }
}
