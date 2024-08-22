// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.EdgeDevices
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Dialogs.EdgeDevices;
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
    public class EdgeDeviceDeleteConfirmationDialogTests : BlazorUnitTest
    {
        private DialogService dialogService;
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task EdgeDeviceDeleteConfirmationDialogMustDeleteDevice()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceClientService.Setup(service => service.DeleteDevice(deviceId))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "DeviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<EdgeDeviceDeleteConfirmationDialog>(string.Empty, parameters));
            cut.Find("#delete").Click();
            var result = await dialogReference.GetReturnValueAsync<bool>();

            // Assert
            _ = result.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EdgeDeviceDeleteConfirmationDialogShouldProcessProblemDetailsExceptionWhenIssueOccursWhenDeletingDevice()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceClientService.Setup(service => service.DeleteDevice(deviceId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "DeviceId", deviceId
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => this.dialogService?.Show<EdgeDeviceDeleteConfirmationDialog>(string.Empty, parameters));
            cut.Find("#delete").Click();

            // Assert
            _ = cut.Find("#delete").TextContent.Should().Be("Delete");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EdgeDeviceDeleteConfirmationDialogMustBeCanceledOnClickOnCancel()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "DeviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<EdgeDeviceDeleteConfirmationDialog>(string.Empty, parameters));
            cut.Find("#cancel").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
