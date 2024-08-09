// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;

    public class PlanningRepositoryTests : BackendUnitTest
    {
        private PlanningRepository planningRepository;

        public override void Setup()
        {
            base.Setup();

            this.planningRepository = new PlanningRepository(DbContext);
        }
    }
}
