// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using IoTHub.Portal.Shared.Models.v10;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Portal.Domain.Repositories;
    using UnitTests.Bases;
    using AutoFixture;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Infrastructure.Services;
    using FluentAssertions;
    using System.Linq;
    using System.Threading;
    using System;

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
            var expectedScheduleDto = Mapper.Map<ScheduleDto>(expectedSchedule);

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
        public async Task UpdateShouldThrowResourceNotFoundException()
        {
            var expectedSchedule = Fixture.Create<Schedule>();
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
            var expectedSchedule = Fixture.Create<Schedule>();
            var expectedScheduleDto = Mapper.Map<ScheduleDto>(expectedSchedule);

            _ = this.mockScheduleRepository.Setup(repository => repository.GetByIdAsync(expectedScheduleDto.Id))
               .ReturnsAsync(expectedSchedule);

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
