// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;

    public class PrincipalRepository : GenericRepository<Principal>, IPrincipalRepository
    {
        public PrincipalRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
