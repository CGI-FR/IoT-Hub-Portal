// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Layer
{
    using AutoFixture;
    using Bunit;
    using System.Collections.Generic;
    using Bunit.TestDoubles;
    using IoTHub.Portal.Client.Components.Layers;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using IoTHub.Portal.Client.Components.Planning;

    internal class EditPlanningTest : BlazorUnitTest
    {
        private Mock<IPlanningClientService> mockPlanningClientService;
        private Mock<IScheduleClientService> mockScheduleClientService;
        private Mock<ILayerClientService> mockLayerClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;

        private FakeNavigationManager mockNavigationManager;
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

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void EditPlanningInit()
        {
            PlanningDto Planning = new PlanningDto
            {
                Day = "SaSu"
            };
            ScheduleDto firstSchedule = new ScheduleDto
            {
                Start = "00:00"
            };

            List<ScheduleDto> ScheduleList = new List<ScheduleDto>
            {
                firstSchedule
            };

            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            // Act
            var cut = RenderComponent<EditPlanning>(
                ComponentParameter.CreateParameter("mode", "New"),
                ComponentParameter.CreateParameter("planning", Planning),
                ComponentParameter.CreateParameter("scheduleList", ScheduleList )
            );

            Assert.AreEqual(cut.Instance.planning.Day, "SaSu");
            Assert.AreEqual(cut.Instance.scheduleList[0].Start, "00:00");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
