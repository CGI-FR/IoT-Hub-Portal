// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    [TestFixture]
    public class DeleteDevicePageTests : BlazorUnitTest
    {
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<ILoRaWanDeviceClientService> mockLoRaWanDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();

            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceClientService.Object);
        }

        [Test]
        public async Task DeleteDevice_NonLoRaDevice_DeviceDeleted()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service => service.DeleteDevice(deviceId))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"deviceID", deviceId},
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<DeleteDevicePage>(string.Empty, parameters));
            cut.WaitForElement("#delete-device").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task DeleteDevice_LoRaDevice_LoRaDeviceDeleted()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLoRaWanDeviceClientService.Setup(service => service.DeleteDevice(deviceId))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"deviceID", deviceId},
                {"IsLoRaWan", true}
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<DeleteDevicePage>(string.Empty, parameters));
            cut.WaitForElement("#delete-device").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
