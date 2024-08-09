// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Server.Controllers.V10;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using UnitTests.Bases;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Domain.Entities;
    using System.Collections.Generic;
    using Microsoft.Azure.Devices.Common.Exceptions;

    [TestFixture]
    public class SchedulesControllerTests : BackendUnitTest
    {
        private Mock<IScheduleService> mockScheduleService;

        private SchedulesController schedulesController;

        public override void Setup()
        {
            base.Setup();

            this.mockScheduleService = MockRepository.Create<IScheduleService>();

            _ = ServiceCollection.AddSingleton(this.mockScheduleService.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.schedulesController = new SchedulesController(Services.GetRequiredService<IScheduleService>());
        }

        [Test]
        public async Task GetScheduleShouldReturnAValueList()
        {
            // Arrange
            var expectedSchedule = Fixture.Create<Schedule>();

            _ = this.mockScheduleService.Setup(x => x.GetSchedules())
                .ReturnsAsync(new List<ScheduleDto>()
                {
                    new ScheduleDto()
                });

            // Act
            var response = await this.schedulesController.GetSchedules();

            // Assert
            Assert.IsNotNull(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetScheduleShouldReturnAValue()
        {
            // Arrange
            var expectedSchedule = Fixture.Create<Schedule>();

            _ = this.mockScheduleService.Setup(service => service.GetSchedule(expectedSchedule.Id))
                .ReturnsAsync(expectedSchedule);

            // Act
            var response = await this.schedulesController.GetSchedule(expectedSchedule.Id);

            // Assert
            Assert.IsNotNull(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetScheduleShouldReturnError()
        {
            // Arrange
            var expectedSchedule = Fixture.Create<Schedule>();

            _ = this.mockScheduleService.Setup(service => service.GetSchedule(expectedSchedule.Id))
                .ThrowsAsync(new DeviceNotFoundException(""));

            // Act
            var response = await this.schedulesController.GetSchedule(expectedSchedule.Id);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundObjectResult>(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldCreateANewEntity()
        {
            // Arrange
            var schedule = Fixture.Create<ScheduleDto>();

            _ = this.mockScheduleService.Setup(service => service.CreateSchedule(schedule))
                .ReturnsAsync(schedule);

            // Act
            _ = await this.schedulesController.CreateScheduleAsync(schedule);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PutShouldUpdateTheSchedule()
        {
            // Arrange
            var schedule = Fixture.Create<ScheduleDto>();

            _ = this.mockScheduleService.Setup(service => service.UpdateSchedule(schedule))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.schedulesController.UpdateSchedule(schedule);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteShouldRemoveTheEntityCommandsAndAvatar()
        {
            // Arrange
            var schedule = Fixture.Create<ScheduleDto>();

            _ = this.mockScheduleService.Setup(service => service.DeleteSchedule(schedule.Id))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.schedulesController.DeleteSchedule(schedule.Id);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
