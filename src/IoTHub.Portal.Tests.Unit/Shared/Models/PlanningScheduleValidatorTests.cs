// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit
{
    [TestFixture]
    public class PlanningScheduleValidatorTests
    {
        [Test]
        public void ValidateTimeline_ContinuousSlots_ReturnsNoError()
        {
            var schedules = new List<ScheduleDto>
            {
                new() { Start = "00:00", End = "12:00" },
                new() { Start = "12:00", End = "23:59" }
            };

            var error = PlanningScheduleValidator.ValidateTimeline(schedules);

            Assert.IsNull(error);
        }

        [Test]
        public void ValidateTimeline_EndBeforeStart_ReturnsError()
        {
            var schedules = new List<ScheduleDto>
            {
                new() { Start = "15:00", End = "08:00" }
            };

            var error = PlanningScheduleValidator.ValidateTimeline(schedules);

            Assert.AreEqual("Each slot's end time must be after its start time.", error);
        }

        [Test]
        public void ValidateTimeline_FirstSlotDoesNotStartAtMidnight_ReturnsError()
        {
            var schedules = new List<ScheduleDto>
            {
                new() { Start = "00:01", End = "23:59" }
            };

            var error = PlanningScheduleValidator.ValidateTimeline(schedules);

            Assert.AreEqual("The first slot must start at 00:00.", error);
        }

        [Test]
        public void ValidateTimeline_LastSlotDoesNotEndAt2359_ReturnsError()
        {
            var schedules = new List<ScheduleDto>
            {
                new() { Start = "00:00", End = "23:58" }
            };

            var error = PlanningScheduleValidator.ValidateTimeline(schedules);

            Assert.AreEqual("The last slot must end at 23:59.", error);
        }

        [Test]
        public void ValidateTimeline_HasGap_ReturnsError()
        {
            var schedules = new List<ScheduleDto>
            {
                new() { Start = "00:00", End = "12:00" },
                new() { Start = "12:01", End = "23:59" }
            };

            var error = PlanningScheduleValidator.ValidateTimeline(schedules);

            Assert.AreEqual("Business-day slots must form a continuous timeline.", error);
        }
    }
}
