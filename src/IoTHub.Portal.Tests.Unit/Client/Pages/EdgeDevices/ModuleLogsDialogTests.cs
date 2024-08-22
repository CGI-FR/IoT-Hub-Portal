// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.EdgeDevices
{
    using System;
    using System.Collections.Generic;
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
    public class ModuleLogsDialogTests : BlazorUnitTest
    {
        private Mock<IEdgeDeviceClientService> edgeDeviceClientServiceMock;

        public override void Setup()
        {
            base.Setup();

            this.edgeDeviceClientServiceMock = MockRepository.Create<IEdgeDeviceClientService>();

            _ = Services.AddSingleton(this.edgeDeviceClientServiceMock.Object);
        }

        [Test]
        public async Task ModuleLogsDialogParametersMustBeCorrect()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            var expectedLogs = new List<IoTEdgeDeviceLog>()
            {
                new(),
                new(),
                new()
            };

            _ = this.edgeDeviceClientServiceMock.Setup(c => c.GetEdgeDeviceLogs(It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)), edgeModule))
                .ReturnsAsync(expectedLogs);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                },
                {
                    "edgeModule", edgeModule
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<ModuleLogsDialog>(string.Empty, parameters));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(4);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ModuleLogsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingLogs()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            _ = this.edgeDeviceClientServiceMock.Setup(c => c.GetEdgeDeviceLogs(It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)), edgeModule))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                },
                {
                    "edgeModule", edgeModule
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<ModuleLogsDialog>(string.Empty, parameters));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(2);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ModuleLogsMustCloseOnCLickOnCloseButton()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            var expectedLogs = new List<IoTEdgeDeviceLog>()
            {
                new()
            };

            _ = this.edgeDeviceClientServiceMock.Setup(c => c.GetEdgeDeviceLogs(It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)), edgeModule))
                .ReturnsAsync(expectedLogs);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                },
                {
                    "edgeModule", edgeModule
                }
            };

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<ModuleLogsDialog>(string.Empty, parameters));
            cut.Find("#close").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
