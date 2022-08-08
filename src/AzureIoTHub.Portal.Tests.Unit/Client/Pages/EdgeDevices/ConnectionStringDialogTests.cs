// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.EdgeDevices
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;
    using AzureIoTHub.Portal.Client.Services;
    using Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class ConnectionStringDialogTests : BlazorUnitTest
    {
        private DialogService dialogService;
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton<ClipboardService>();
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task ConnectionStringDialogMustShowEnrollmentCredentials()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetEnrollmentCredentials(deviceId))
                .ReturnsAsync(new EnrollmentCredentials());

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            // Act
            await cut.InvokeAsync(() => this.dialogService?.Show<ConnectionStringDialog>(string.Empty, parameters));
            _ = cut.WaitForElement("div.mud-paper");

            // Assert
            _ = cut.FindAll("div.mud-grid-item").Count.Should().Be(4);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ConnectionStringDialogMustBeCancelledWhenProblemDetailsOccurs()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetEnrollmentCredentials(deviceId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<ConnectionStringDialog>(string.Empty, parameters));
            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ConnectionStringDialogMustBeCancelledOnClickOnCancel()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetEnrollmentCredentials(deviceId))
                .ReturnsAsync(new EnrollmentCredentials());

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<ConnectionStringDialog>(string.Empty, parameters));
            cut.WaitForElement("#cancel").Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
