// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Devices
{
    using System;
    using System.Threading.Tasks;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Pages.Devices;
    using Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class ConnectionStringDialogTests : BlazorUnitTest
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
                .ReturnsAsync(new EnrollmentCredentials());

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            // Act
            await cut.InvokeAsync(() => service?.Show<ConnectionStringDialog>(string.Empty, parameters));

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
            await cut.InvokeAsync(() => dialogReference = service?.Show<ConnectionStringDialog>(string.Empty, parameters));

            var result = await dialogReference.Result;

            // Assert
            cut.WaitForAssertion(() => result.Cancelled.Should().BeFalse());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
