// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.DeviceConfigurations
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.DeviceConfigurations;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class DeleteDeviceConfigurationTests : BlazorUnitTest
    {
        private DialogService dialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IDeviceConfigurationsClientService> mockDeviceConfigurationsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockDeviceConfigurationsClientService = MockRepository.Create<IDeviceConfigurationsClientService>();

            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockDeviceConfigurationsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task DeleteDeviceConfigurationShouldDeleteConfiguration()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var configurationName = Guid.NewGuid().ToString();

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.DeleteDeviceConfiguration(It.Is<string>(s => configurationId.Equals(s, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "configurationId", configurationId
                },
                {
                    "configurationName", configurationName
                }
            };

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            IDialogReference dialogReference = null;

            _ = await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<DeleteDeviceConfiguration>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#delete-device-configuration"));

            // Act
            cut.Find("#delete-device-configuration").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeFalse();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task DeleteConcentratorPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnDeletingConcentrator()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var configurationName = Guid.NewGuid().ToString();

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.DeleteDeviceConfiguration(It.Is<string>(s => configurationId.Equals(s, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "configurationId", configurationId
                },
                {
                    "configurationName", configurationName
                }
            };

            _ = await cut.InvokeAsync(() => this.dialogService?.Show<DeleteDeviceConfiguration>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#delete-device-configuration"));

            // Act
            cut.Find("#delete-device-configuration").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
