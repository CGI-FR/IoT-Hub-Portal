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
    public class PlanningsControllerTests : BackendUnitTest
    {
        private Mock<IPlanningService> mockPlanningService;

        private PlanningsController planningsController;

        public override void Setup()
        {
            base.Setup();

            this.mockPlanningService = MockRepository.Create<IPlanningService>();

            _ = ServiceCollection.AddSingleton(this.mockPlanningService.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.planningsController = new PlanningsController(Services.GetRequiredService<IPlanningService>());
        }

        [Test]
        public async Task GetPlanningShouldReturnAValueList()
        {
            // Arrange
            var expectedPlanning = Fixture.Create<Planning>();

            _ = this.mockPlanningService.Setup(x => x.GetPlannings())
                .ReturnsAsync(new List<PlanningDto>()
                {
                    new PlanningDto()
                });

            // Act
            var response = await this.planningsController.GetPlannings();

            // Assert
            Assert.IsNotNull(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPlanningShouldReturnAValue()
        {
            // Arrange
            var expectedPlanning = Fixture.Create<Planning>();

            _ = this.mockPlanningService.Setup(service => service.GetPlanning(expectedPlanning.Id))
                .ReturnsAsync(expectedPlanning);

            // Act
            var response = await this.planningsController.GetPlanning(expectedPlanning.Id);

            // Assert
            Assert.IsNotNull(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetPlanningShouldReturnError()
        {
            // Arrange
            var expectedPlanning = Fixture.Create<Planning>();

            _ = this.mockPlanningService.Setup(service => service.GetPlanning(expectedPlanning.Id))
                .ThrowsAsync(new DeviceNotFoundException(""));

            // Act
            var response = await this.planningsController.GetPlanning(expectedPlanning.Id);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundObjectResult>(response);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldCreateANewEntity()
        {
            // Arrange
            var planning = Fixture.Create<PlanningDto>();

            _ = this.mockPlanningService.Setup(service => service.CreatePlanning(planning))
                .ReturnsAsync(planning);

            // Act
            _ = await this.planningsController.CreatePlanningAsync(planning);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task PutShouldUpdateThePlanning()
        {
            // Arrange
            var planning = Fixture.Create<PlanningDto>();

            _ = this.mockPlanningService.Setup(service => service.UpdatePlanning(planning))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.planningsController.UpdatePlanning(planning);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteShouldRemoveTheEntityCommandsAndAvatar()
        {
            // Arrange
            var planning = Fixture.Create<PlanningDto>();

            _ = this.mockPlanningService.Setup(service => service.DeletePlanning(planning.Id))
                .Returns(Task.CompletedTask);

            // Act
            _ = await this.planningsController.DeletePlanning(planning.Id);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
