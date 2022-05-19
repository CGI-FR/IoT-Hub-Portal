// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using AzureIoTHub.Portal.Client.Pages.Edge_Devices;
    using AzureIoTHub.Portal.Client.Shared;
    using Bunit;
    using Bunit.TestDoubles;
    using Helpers;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class EdgeDeviceDetailPageTests : IDisposable
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
#pragma warning restore CA2213 // Disposable fields should be disposed

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private readonly string mockdeviceId = Guid.NewGuid().ToString();

        private FakeNavigationManager mockNavigationManager;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = this.testContext.Services.AddMockHttpClient();

            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);

            this.mockSnackbarService = this.mockRepository.Create<ISnackbar>();
            _ = this.testContext.Services.AddSingleton(this.mockSnackbarService.Object);

            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

            this.mockNavigationManager = this.testContext.Services.GetRequiredService<FakeNavigationManager>();
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(new IoTEdgeDevice() { ConnectionState = "false" });

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            var returnButton = cut.WaitForElement("#returnButton");

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/edge/devices", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void ClickOnSaveShouldPutEdgeDeviceDetails()
        {
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "false",
                Type = "Other"
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = this.mockHttpClient
                .When(HttpMethod.Put, $"/api/edge/devices/{this.mockdeviceId}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<IoTEdgeDevice>(jsonContent.Value);
                    var edgeDevice = jsonContent.Value as IoTEdgeDevice;

                    Assert.AreEqual(mockIoTEdgeDevice.DeviceId, edgeDevice.DeviceId);
                    Assert.AreEqual(mockIoTEdgeDevice.ConnectionState, edgeDevice.ConnectionState);
                    Assert.AreEqual(mockIoTEdgeDevice.Type, edgeDevice.Type);

                    return true;
                })
                .RespondText(string.Empty);


            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            Thread.Sleep(2500);

            var saveButton = cut.WaitForElement("#saveButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add($"Device {mockdeviceId} has been successfully updated!", Severity.Success, null));

            // Act
            saveButton.Click();

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
