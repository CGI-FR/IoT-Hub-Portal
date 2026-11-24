// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using ResourceNotFoundException = Portal.Domain.Exceptions.ResourceNotFoundException;

    [TestFixture]
    public class ScheduleServiceTests : BackendUnitTest
    {
        private Mock<IScheduleRepository> mockScheduleRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private IScheduleService scheduleService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.mockScheduleRepository = MockRepository.Create<IScheduleRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();

            _ = ServiceCollection.AddSingleton(this.mockScheduleRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IScheduleService, ScheduleService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.scheduleService = Services.GetRequiredService<IScheduleService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateNewEntity()
        {
            var expectedSchedule = Fixture.Create<Schedule>();
            expectedSchedule.Start = "00:00";
            expectedSchedule.End = "23:59";
            var expectedScheduleDto = Mapper.Map<ScheduleDto>(expectedSchedule);

            _ = this.mockScheduleRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<Schedule, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Schedule>());

            _ = this.mockScheduleRepository.Setup(repository => repository.InsertAsync(It.IsAny<Schedule>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await this.scheduleService.CreateSchedule(expectedScheduleDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedScheduleDto.Start, result.Start);
            Assert.AreEqual(expectedScheduleDto.End, result.End);
            Assert.AreEqual(expectedScheduleDto.CommandId, result.CommandId);
            Assert.AreEqual(expectedScheduleDto.PlanningId, result.PlanningId);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ReplacePlanningSchedulesShouldSaveAValidTimeline()
        {
            var planningId = Guid.NewGuid().ToString();
            var schedules = new List<ScheduleDto>
            {
                new() { Id = Guid.NewGuid().ToString(), Start = "00:00", End = "12:00" },
                new() { Id = Guid.NewGuid().ToString(), Start = "12:00", End = "23:59" }
            };

            _ = this.mockScheduleRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<Schedule, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Schedule>());
            _ = this.mockScheduleRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<object>()))
                .ReturnsAsync((Schedule)null);
            _ = this.mockScheduleRepository.Setup(repository => repository.InsertAsync(It.IsAny<Schedule>()))
                .Returns(Task.CompletedTask);
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            await this.scheduleService.ReplacePlanningSchedules(planningId, schedules);

            Assert.That(schedules.All(schedule => schedule.PlanningId == planningId), Is.True);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ReplacePlanningSchedulesShouldRejectAnInvalidTimeline()
        {
            var schedules = new List<ScheduleDto>
            {
                new() { Start = "15:00", End = "08:00" }
            };

            var act = () => this.scheduleService.ReplacePlanningSchedules(Guid.NewGuid().ToString(), schedules);

            _ = await act.Should().ThrowAsync<InvalidPlanningScheduleException>()
                .WithMessage("Each slot's end time must be after its start time.");
        }

        [Test]
        public async Task UpdateShouldThrowResourceNotFoundException()
        {
            var expectedSchedule = Fixture.Create<Schedule>();
            expectedSchedule.Start = "08:00";
            expectedSchedule.End = "09:00";
            var expectedScheduleDto = Mapper.Map<ScheduleDto>(expectedSchedule);

            _ = this.mockScheduleRepository.Setup(repository => repository.GetByIdAsync(expectedScheduleDto.Id))
                .ReturnsAsync((Schedule)null);

            var act = () => this.scheduleService.UpdateSchedule(expectedScheduleDto);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The schedule with id {expectedSchedule.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteExistigScheduleSuccessfully()
        {
            var planningId = Guid.NewGuid().ToString();
            var expectedSchedule = new Schedule
            {
                Id = Guid.NewGuid().ToString(),
                PlanningId = planningId,
                Start = "01:00",
                End = "02:00"
            };
            var remainingSchedule = new Schedule
            {
                Id = Guid.NewGuid().ToString(),
                PlanningId = planningId,
                Start = "00:00",
                End = "23:59"
            };
            var expectedScheduleDto = Mapper.Map<ScheduleDto>(expectedSchedule);

            _ = this.mockScheduleRepository.Setup(repository => repository.GetByIdAsync(expectedScheduleDto.Id))
               .ReturnsAsync(expectedSchedule);
            _ = this.mockScheduleRepository.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<Schedule, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Schedule> { expectedSchedule, remainingSchedule });

            this.mockScheduleRepository.Setup(repository => repository.Delete(expectedScheduleDto.Id))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            await this.scheduleService.DeleteSchedule(expectedScheduleDto.Id);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteShuldThrowResourceNotFoundException()
        {
            var expectedSchedule = Fixture.Create<Schedule>();
            var expectedScheduleDto = Mapper.Map<ScheduleDto>(expectedSchedule);

            _ = this.mockScheduleRepository.Setup(repository => repository.GetByIdAsync(expectedScheduleDto.Id))
                .ReturnsAsync((Schedule)null);

            _ = this.mockScheduleRepository.Setup(repository => repository.GetByIdAsync(expectedScheduleDto.Id))
                .ReturnsAsync((Schedule)null);

            var act = () => this.scheduleService.DeleteSchedule(expectedScheduleDto.Id);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The schedule with id {expectedSchedule.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetScheduleShouldReturnValue()
        {
            var expectedSchedule = Fixture.Create<Schedule>();
            var expectedScheduleDto = Mapper.Map<ScheduleDto>(expectedSchedule);

            _ = this.mockScheduleRepository.Setup(repository => repository.GetByIdAsync(expectedScheduleDto.Id))
               .ReturnsAsync(expectedSchedule);

            var result = await this.scheduleService.GetSchedule(expectedScheduleDto.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedScheduleDto.Start, result.Start);
            Assert.AreEqual(expectedScheduleDto.End, result.End);
            Assert.AreEqual(expectedScheduleDto.CommandId, result.CommandId);
            Assert.AreEqual(expectedScheduleDto.PlanningId, result.PlanningId);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetScheduleShouldThrowResourceNotFoundException()
        {
            var expectedSchedule = Fixture.Create<Schedule>();
            var expectedScheduleDto = Mapper.Map<ScheduleDto>(expectedSchedule);

            _ = this.mockScheduleRepository.Setup(repository => repository.GetByIdAsync(expectedScheduleDto.Id))
                .ReturnsAsync((Schedule)null);

            var act = () => this.scheduleService.GetSchedule(expectedScheduleDto.Id);

            _ = await act.Should().ThrowAsync<ResourceNotFoundException>($"The schedule with id {expectedSchedule.Id} doesn't exist");

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetSchedulesShouldReturnScheduleList()
        {
            var expectedTotalSchedulesCount = 50;
            var expectedDevices = Fixture.CreateMany<Schedule>(expectedTotalSchedulesCount).ToList();

            await DbContext.AddRangeAsync(expectedDevices);
            _ = await DbContext.SaveChangesAsync();

            _ = this.mockScheduleRepository.Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDevices);

            var result = await this.scheduleService.GetSchedules();

            _ = result.Count().Should().Be(expectedTotalSchedulesCount);
            MockRepository.VerifyAll();
        }
    }
}
