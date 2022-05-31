// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
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
    using Newtonsoft.Json;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class EdgeDeviceDetailPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;

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
                ConnectionState = "Connected",
                Type = "Other"
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = this.mockHttpClient
                .When(HttpMethod.Put, $"/api/edge/devices/{this.mockdeviceId}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeDevice>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeDevice>;

                    Assert.IsAssignableFrom<IoTEdgeDevice>(objectContent.Value);
                    var edgeDevice = objectContent.Value as IoTEdgeDevice;

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

            _ = this.mockSnackbarService.Setup(c => c.Add($"Device {this.mockdeviceId} has been successfully updated!", Severity.Success, null));

            // Act
            saveButton.Click();
            Thread.Sleep(2500);

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnSaveShouldDisplaySnackbarIfValidationError()
        {
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other"
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = this.mockHttpClient
                .When(HttpMethod.Put, $"/api/edge/devices/{this.mockdeviceId}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeDevice>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeDevice>;

                    Assert.IsAssignableFrom<IoTEdgeDevice>(objectContent.Value);
                    var edgeDevice = objectContent.Value as IoTEdgeDevice;

                    Assert.AreEqual(mockIoTEdgeDevice.DeviceId, edgeDevice.DeviceId);
                    Assert.AreEqual(mockIoTEdgeDevice.ConnectionState, edgeDevice.ConnectionState);
                    Assert.AreEqual(mockIoTEdgeDevice.Type, edgeDevice.Type);

                    return true;
                })
                .Respond(System.Net.HttpStatusCode.BadRequest);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            Thread.Sleep(2500);

            var saveButton = cut.WaitForElement("#saveButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add("One or more validation errors occurred", Severity.Error, null));

            // Act
            saveButton.Click();
            Thread.Sleep(2500);

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnSaveShouldDisplaySnackbarIfUnexpectedError()
        {
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other"
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = this.mockHttpClient
                .When(HttpMethod.Put, $"/api/edge/devices/{this.mockdeviceId}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeDevice>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeDevice>;

                    Assert.IsAssignableFrom<IoTEdgeDevice>(objectContent.Value);
                    var edgeDevice = objectContent.Value as IoTEdgeDevice;

                    Assert.AreEqual(mockIoTEdgeDevice.DeviceId, edgeDevice.DeviceId);
                    Assert.AreEqual(mockIoTEdgeDevice.ConnectionState, edgeDevice.ConnectionState);
                    Assert.AreEqual(mockIoTEdgeDevice.Type, edgeDevice.Type);

                    return true;
                })
                .Respond(System.Net.HttpStatusCode.NotFound);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            Thread.Sleep(2500);

            var saveButton = cut.WaitForElement("#saveButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add("Something unexpected occurred", Severity.Warning, null));

            // Act
            saveButton.Click();
            Thread.Sleep(2500);

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnRebootShouldRebootModule()
        {
            var mockIoTEdgeModule = new IoTEdgeModule()
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other",
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule}
            };


            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = this.mockHttpClient
                .When(HttpMethod.Post, $"/api/edge/devices/{mockIoTEdgeDevice.DeviceId}/{mockIoTEdgeModule.ModuleName}/RestartModule")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeModule>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeModule>;

                    Assert.IsAssignableFrom<IoTEdgeModule>(objectContent.Value);
                    var edgeModule = objectContent.Value as IoTEdgeModule;

                    Assert.AreEqual(mockIoTEdgeModule.ModuleName, edgeModule.ModuleName);

                    return true;
                })
                .Respond(System.Net.HttpStatusCode.OK, new StringContent(
                    JsonConvert.SerializeObject(new C2Dresult()
                    {
                        Payload = "ABC",
                        Status = 200
                    }),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            Thread.Sleep(2500);

            var rebootButton = cut.WaitForElement("#rebootModule");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add("Command successfully executed.", Severity.Success, null));

            // Act
            rebootButton.Click();
            Thread.Sleep(2500);

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnRebootShouldDisplaySnackbarIfError()
        {
            var mockIoTEdgeModule = new IoTEdgeModule()
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other",
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule}
            };


            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = this.mockHttpClient
                .When(HttpMethod.Post, $"/api/edge/devices/{mockIoTEdgeDevice.DeviceId}/{mockIoTEdgeModule.ModuleName}/RestartModule")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeModule>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeModule>;

                    Assert.IsAssignableFrom<IoTEdgeModule>(objectContent.Value);
                    var edgeModule = objectContent.Value as IoTEdgeModule;

                    Assert.AreEqual(mockIoTEdgeModule.ModuleName, edgeModule.ModuleName);

                    return true;
                })
                .Respond(System.Net.HttpStatusCode.InternalServerError, new StringContent(
                    JsonConvert.SerializeObject(new C2Dresult()
                    {
                        Payload = "ABC",
                        Status = 500
                    }),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            Thread.Sleep(2500);

            var rebootButton = cut.WaitForElement("#rebootModule");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>()));

            // Act
            rebootButton.Click();
            Thread.Sleep(2500);

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnLogsShouldDisplayLogs()
        {
            var mockIoTEdgeModule = new IoTEdgeModule()
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other",
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule}
            };


            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var logsButton = cut.WaitForElement("#showLogs");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ModuleLogsDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            // Act
            logsButton.Click();

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnConnectShouldDisplayDeviceCredentials()
        {
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other",
            };


            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var connectButton = cut.WaitForElement("#connectButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ConnectionStringDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            // Act
            connectButton.Click();

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other",
            };


            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var deleteButton = cut.WaitForElement("#deleteButton");

            var mockDialogReference = this.mockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService.Setup(c => c.Show<EdgeDeviceDeleteConfirmationDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            deleteButton.Click();

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();

            //Thread.Sleep(2500);
            //cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/edge/devices", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndRedirectIfConfirmed()
        {
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other",
            };


            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var deleteButton = cut.WaitForElement("#deleteButton");

            var mockDialogReference = this.mockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService.Setup(c => c.Show<EdgeDeviceDeleteConfirmationDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            deleteButton.Click();

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();

            // Thread.Sleep(2500);
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/edge/devices", StringComparison.OrdinalIgnoreCase));
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
