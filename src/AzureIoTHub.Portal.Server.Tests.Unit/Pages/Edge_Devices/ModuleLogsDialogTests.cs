// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Edge_Devices
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Client.Pages.Edge_Devices;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Models.v10;
    using Bunit;
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
        private Mock<IDialogService> mockDialogService;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.edgeDeviceClientServiceMock = this.mockRepository.Create<IEdgeDeviceClientService>();
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.Services.AddSingleton(this.edgeDeviceClientServiceMock.Object);

            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);

            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
        }



        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void ModuleLogsDialogParametersMustBeCorrect()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var edgeModule = new IoTEdgeModule
            {
                Version = "1.0",
                ModuleName = Guid.NewGuid().ToString()
            };

            var expectedLog = new IoTEdgeDeviceLog
            {
                Id = deviceId,
                Text = Guid.NewGuid().ToString(),
                LogLevel = 1,
                TimeStamp = DateTime.UtcNow
            };

            var expectedLogs = new List<IoTEdgeDeviceLog>()
            {
                expectedLog
            };

            _ = edgeDeviceClientServiceMock.Setup(c => c.GetEdgeDeviceLogs(It.Is<string>(x => x.Equals(deviceId, StringComparison.Ordinal)), It.Is<IoTEdgeModule>(x => x.Equals(edgeModule))))
                .ReturnsAsync(expectedLogs);

            // Act
            var cut = RenderComponent<ModuleLogsDialog>(ComponentParameter.CreateParameter("deviceId", deviceId), ComponentParameter.CreateParameter("edgeModule", edgeModule));

            // Assert
            _ = cut.Instance.deviceId.Should().Be(deviceId);
            _ = cut.Instance.edgeModule.Should().Be(edgeModule);
        }
    }
}
