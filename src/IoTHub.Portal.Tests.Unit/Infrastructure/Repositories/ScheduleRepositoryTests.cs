// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Repositories
{
    public class ScheduleRepositoryTests : BackendUnitTest
    {
        private ScheduleRepository scheduleRepository;

        public override void Setup()
        {
            base.Setup();

            this.scheduleRepository = new ScheduleRepository(DbContext);
        }
    }
}
