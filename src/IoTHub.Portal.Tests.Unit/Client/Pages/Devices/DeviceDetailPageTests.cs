// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using Portal.Shared.Security;

    [TestFixture]
    public class DeviceDetailPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private FakeNavigationManager mockNavigationManager;

        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<ILoRaWanDeviceClientService> mockLoRaWanDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();
            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();
            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceClientService.Object);

            _ = Services.AddSingleton<IDeviceLayoutService, DeviceLayoutService>();

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false, CloudProvider = "Azure" });

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();

            _ = this.mockPermissionsService.Setup(service => service.GetUserPermissions())
                .ReturnsAsync(new[] { PortalPermissions.DeviceRead, PortalPermissions.DeviceWrite, PortalPermissions.DeviceExecute });
        }

        [Test]
        public void DeviceDetailPageShouldRenderCorrectly()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDevice(deviceId))
                .ReturnsAsync(new DeviceDetails());

            _ = this.mockDeviceClientService
                .Setup(service => service.GetDeviceProperties(deviceId))
                .ReturnsAsync(new List<DevicePropertyValue>());

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModel(null))
                .ReturnsAsync(new DeviceModelDto());

            // Act
            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", deviceId));

            // Assert
            cut.WaitForAssertion(() => cut.Find("#saveButton").TextContent.Should().Be("Save"));
        }
    }
}
