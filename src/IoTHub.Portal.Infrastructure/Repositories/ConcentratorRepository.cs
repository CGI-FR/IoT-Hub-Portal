// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    public class ConcentratorRepository : GenericRepository<Concentrator>, IConcentratorRepository
    {
        public ConcentratorRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
