// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Planning
{
    using Portal.Shared.Security;

    [TestFixture]
    internal class CreatePlanningsPageTest : BlazorUnitTest
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

            _ = this.mockPermissionsService.Setup(service => service.GetUserPermissions())
                .ReturnsAsync(new[] { PortalPermissions.PlanningRead, PortalPermissions.PlanningWrite });
        }

        [Test]
        public void OnInitializedAsync_CreatePlanningPage()
        {
            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            // Act
            var cut = RenderComponent<CreatePlanningsPage>();

            // Assert
            Assert.AreEqual(cut.Instance.Planning.DayOff, DaysEnumFlag.DaysOfWeek.Saturday | DaysEnumFlag.DaysOfWeek.Sunday);
            Assert.AreEqual(cut.Instance.ScheduleList[0].Start, "00:00");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void CreatePlanningPage_SaveButtonIsDisplayed()
        {
            _ = this.mockLayerClientService.Setup(service => service.GetLayers())
                .ReturnsAsync(new List<LayerDto>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            // Act
            var cut = RenderComponent<CreatePlanningsPage>();

            // Wait for the save button to be rendered
            var saveButton = cut.WaitForElement("#saveButton");

            // Assert
            Assert.IsNotNull(saveButton);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
