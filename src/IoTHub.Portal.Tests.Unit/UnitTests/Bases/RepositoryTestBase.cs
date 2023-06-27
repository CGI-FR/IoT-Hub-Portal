// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.UnitTests.Bases
{
    using IoTHub.Portal.Infrastructure.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public abstract class RepositoryTestBase
    {
        protected static PortalDbContext SetupDbContext()
        {
            var contextOptions = new DbContextOptionsBuilder<PortalDbContext>()
                   .UseInMemoryDatabase("TestContext")
                   .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                   .Options;

            return new PortalDbContext(contextOptions);
        }
    }
}
