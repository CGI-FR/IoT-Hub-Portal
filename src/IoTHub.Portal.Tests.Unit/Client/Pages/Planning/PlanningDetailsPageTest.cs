// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Planning
{
    using Bunit;
    using IoTHub.Portal.Client.Pages.Planning;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using System.Collections.Generic;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using AutoFixture;

    [TestFixture]
    internal class PlanningDetailsPageTest : BlazorUnitTest
    {
        private Mock<IPlanningClientService> mockPlanningClientService;
        private Mock<IScheduleClientService> mockScheduleClientService;
        private Mock<ILayerClientService> mockLayerClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;
        public override void Setup()
        {
            base.Setup();

            this.mockScheduleClientService = MockRepository.Create<IScheduleClientService>();
            this.mockPlanningClientService = MockRepository.Create<IPlanningClientService>();
            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockScheduleClientService.Object);
            _ = Services.AddSingleton(this.mockPlanningClientService.Object);
            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
        }

        [Test]
        public void OnInitializedAsync_CreatePlanningPage()
        {
            var expectedPlanningDto = Fixture.Create<PlanningDto>();
            expectedPlanningDto.CommandId = null;

            var expectedScheduleDto1 = Fixture.Create<ScheduleDto>();
            var expectedScheduleDto2 = Fixture.Create<ScheduleDto>();

            var listExpectedSchedule = new List<ScheduleDto>
            {
                expectedScheduleDto1,
                expectedScheduleDto2
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            _ = this.mockPlanningClientService.Setup(service => service.GetPlanning(It.IsAny<string>()))
                .ReturnsAsync(expectedPlanningDto);

            _ = this.mockScheduleClientService.Setup(service => service.GetSchedules())
                .ReturnsAsync(listExpectedSchedule);

            // Act
            var cut = RenderComponent<PlanningDetailsPage>();

            // Assert
            Assert.AreEqual(cut.Instance.Planning, expectedPlanningDto);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
