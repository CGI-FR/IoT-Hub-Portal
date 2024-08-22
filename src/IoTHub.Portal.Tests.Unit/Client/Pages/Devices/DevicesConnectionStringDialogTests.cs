// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    [TestFixture]
    public class DevicesConnectionStringDialogTests : BlazorUnitTest
    {
        private Mock<IDeviceClientService> mockDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();

            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton<ClipboardService>();
        }

        [Test]
        public async Task ConnectionStringDialogMustBeRenderedOnShow()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service => service.GetEnrollmentCredentials(deviceId))
                .ReturnsAsync(new DeviceCredentials());

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<DevicesConnectionStringDialog>(string.Empty, parameters));

            // Assert
            cut.WaitForAssertion(() => cut.Find("div.mud-dialog-container").Should().NotBeNull());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingCredentials()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service => service.GetEnrollmentCredentials(deviceId))
                 .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<DevicesConnectionStringDialog>(string.Empty, parameters));

            var result = await dialogReference.Result;

            // Assert
            cut.WaitForAssertion(() => result.Canceled.Should().BeFalse());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
