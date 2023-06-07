// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Repositories;

    public class LabelRepository : GenericRepository<Label>, ILabelRepository
    {
        public LabelRepository(PortalDbContext context) : base(context)
        {
        }
    }
}
