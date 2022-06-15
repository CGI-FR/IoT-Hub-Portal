// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Edge_Devices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.Edge_Devices;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Models.v10;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class ModuleLogsDialogTests
    {

#pragma warning disable CA2213 // Disposable fields should be disposed
        private Bunit.TestContext testContext;
#pragma warning restore CA2213 // Disposable fields should be disposed

        private MockRepository mockRepository;
        private Mock<IEdgeDeviceClientService> edgeDeviceClientServiceMock;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.edgeDeviceClientServiceMock = this.mockRepository.Create<IEdgeDeviceClientService>();

            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.Services.AddSingleton(this.edgeDeviceClientServiceMock.Object);

            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
        }



        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public async Task ModuleLogsDialogParametersMustBeCorrect()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            var expectedLogs = new List<IoTEdgeDeviceLog>()
            {
                new(),
                new(),
                new()
            };

            _ = this.edgeDeviceClientServiceMock.Setup(c => c.GetEdgeDeviceLogs(It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)), It.Is<IoTEdgeModule>(x => x.Equals(edgeModule))))
                .ReturnsAsync(expectedLogs);

            var cut = RenderComponent<MudDialogProvider>();
            var service = this.testContext.Services.GetService<IDialogService>() as DialogService;

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
            await cut.InvokeAsync(() => service?.Show<ModuleLogsDialog>(string.Empty, parameters));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(4);
        }

        [Test]
        public async Task ModuleLogsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingLogs()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            _ = this.edgeDeviceClientServiceMock.Setup(c => c.GetEdgeDeviceLogs(It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)), It.Is<IoTEdgeModule>(x => x.Equals(edgeModule))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();
            var service = this.testContext.Services.GetService<IDialogService>() as DialogService;

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
            await cut.InvokeAsync(() => service?.Show<ModuleLogsDialog>(string.Empty, parameters));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(2);
        }

        [Test]
        public async Task ModuleLogsMustCloseOnCLickOnCloseButton()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            var expectedLogs = new List<IoTEdgeDeviceLog>()
            {
                new()
            };

            _ = this.edgeDeviceClientServiceMock.Setup(c => c.GetEdgeDeviceLogs(It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)), It.Is<IoTEdgeModule>(x => x.Equals(edgeModule))))
                .ReturnsAsync(expectedLogs);

            var cut = RenderComponent<MudDialogProvider>();
            var service = this.testContext.Services.GetService<IDialogService>() as DialogService;

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
            await cut.InvokeAsync(() => dialogReference = service?.Show<ModuleLogsDialog>(string.Empty, parameters));
            cut.Find("#close").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeTrue();
        }
    }
}
