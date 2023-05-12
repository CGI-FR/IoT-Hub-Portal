// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Repositories.AWS
{
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Domain.Repositories.AWS;

    public class ThingTypeTagRepository : GenericRepository<ThingTypeTag>, IThingTypeTagRepository
    {
        public ThingTypeTagRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
