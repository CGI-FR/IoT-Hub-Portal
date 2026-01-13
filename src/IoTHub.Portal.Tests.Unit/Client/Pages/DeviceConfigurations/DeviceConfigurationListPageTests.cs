// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.DeviceConfigurations
{
    using Portal.Shared.Security;

    [TestFixture]
    public class DeviceConfigurationListPageTests : BlazorUnitTest
    {
        private Mock<IDeviceConfigurationsClientService> mockDeviceConfigurationsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceConfigurationsClientService = MockRepository.Create<IDeviceConfigurationsClientService>();

            _ = Services.AddSingleton(this.mockDeviceConfigurationsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockPermissionsService.Setup(service => service.GetUserPermissions())
                .ReturnsAsync(new[] { PortalPermissions.DeviceConfigurationRead, PortalPermissions.DeviceConfigurationWrite });
        }

        [Test]
        public void DeviceConfigurationListPageShouldLoadAndShowConfigurations()
        {
            // Arrange
            var expectedConfigurations = Fixture.Build<ConfigListItem>().CreateMany(3).ToList();

            _ = this.mockDeviceConfigurationsClientService.Setup(service => service.GetDeviceConfigurations())
                .ReturnsAsync(expectedConfigurations);

            // Act
            var cut = RenderComponent<DeviceConfigurationListPage>();
            cut.WaitForAssertion(() => cut.Find("#device-configurations-listing"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            _ = cut.FindAll("table tbody tr").Count.Should().Be(3);
            MockRepository.VerifyAll();
        }

        [Test]
        public void DeviceConfigurationListPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingConfigurations()
        {
            // Arrange

            _ = this.mockDeviceConfigurationsClientService.Setup(service => service.GetDeviceConfigurations())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceConfigurationListPage>();
            cut.WaitForAssertion(() => cut.Find("#device-configurations-listing"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            _ = cut.FindAll("table tbody tr").Count.Should().Be(1);
            MockRepository.VerifyAll();
        }

        [Test]
        public void ClickToItemShouldRedirectToConfigurationDetailsPage()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();

            var configurations = new List<ConfigListItem>
            {
                new()
                {
                    ConfigurationID = configurationId
                }
            };

            _ = this.mockDeviceConfigurationsClientService.Setup(service => service.GetDeviceConfigurations())
                .ReturnsAsync(configurations);

            var cut = RenderComponent<DeviceConfigurationListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/device-configurations/{configurationId}"));
        }
    }
}
