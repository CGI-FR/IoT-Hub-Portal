// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.EdgeDevices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Threading;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;
    using AzureIoTHub.Portal.Client.Shared;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Mocks;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class EdgeDeviceDetailPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private readonly string mockdeviceId = Guid.NewGuid().ToString();

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(new IoTEdgeDevice() { ConnectionState = "false" });

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            var returnButton = cut.WaitForElement("#returnButton");

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/edge/devices"));
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

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = MockHttpClient
                .When(HttpMethod.Put, $"/api/edge/devices/{this.mockdeviceId}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeDevice>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeDevice>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<IoTEdgeDevice>(objectContent.Value);
                    var edgeDevice = objectContent.Value as IoTEdgeDevice;
                    Assert.IsNotNull(edgeDevice);

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

            _ = this.mockSnackbarService.Setup(c => c.Add($"Device {this.mockdeviceId} has been successfully updated!", Severity.Success, null)).Returns((Snackbar)null);

            // Act
            saveButton.Click();
            Thread.Sleep(2500);

            // Assert            
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockRepository.VerifyAll();
        }

        [Test]
        public void EdgeDeviceDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadDevice()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            // Assert
            cut.WaitForAssertion(() => cut.Find("form").Should().NotBeNull());
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void EdgeDeviceDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnUpdateDevice()
        {
            // Arrange
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other"
            };

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = MockHttpClient
                .When(HttpMethod.Put, $"/api/edge/devices/{this.mockdeviceId}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeDevice>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeDevice>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<IoTEdgeDevice>(objectContent.Value);
                    var edgeDevice = objectContent.Value as IoTEdgeDevice;
                    Assert.IsNotNull(edgeDevice);

                    Assert.AreEqual(mockIoTEdgeDevice.DeviceId, edgeDevice.DeviceId);
                    Assert.AreEqual(mockIoTEdgeDevice.ConnectionState, edgeDevice.ConnectionState);
                    Assert.AreEqual(mockIoTEdgeDevice.Type, edgeDevice.Type);

                    return true;
                })
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            var cut = RenderComponent<EdgeDeviceDetailPage>(
                ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("form").Should().NotBeNull());

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
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


            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = MockHttpClient
                .When(HttpMethod.Post, $"/api/edge/devices/{mockIoTEdgeDevice.DeviceId}/{mockIoTEdgeModule.ModuleName}/RestartModule")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeModule>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeModule>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<IoTEdgeModule>(objectContent.Value);
                    var edgeModule = objectContent.Value as IoTEdgeModule;
                    Assert.IsNotNull(edgeModule);

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

            _ = this.mockSnackbarService.Setup(c => c.Add("Command successfully executed.", Severity.Success, null)).Returns((Snackbar)null);

            // Act
            rebootButton.Click();
            Thread.Sleep(2500);

            // Assert            
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockRepository.VerifyAll();
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


            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = MockHttpClient
                .When(HttpMethod.Post, $"/api/edge/devices/{mockIoTEdgeDevice.DeviceId}/{mockIoTEdgeModule.ModuleName}/RestartModule")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeModule>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeModule>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<IoTEdgeModule>(objectContent.Value);
                    var edgeModule = objectContent.Value as IoTEdgeModule;
                    Assert.IsNotNull(edgeModule);

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

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>())).Returns((Snackbar)null);

            // Act
            rebootButton.Click();
            Thread.Sleep(2500);

            // Assert            
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockRepository.VerifyAll();
        }

        [Test]
        public void EdgeDeviceDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnClickOnReboot()
        {
            // Arrange
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


            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            _ = MockHttpClient
                .When(HttpMethod.Post, $"/api/edge/devices/{mockIoTEdgeDevice.DeviceId}/{mockIoTEdgeModule.ModuleName}/RestartModule")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<IoTEdgeModule>>(m.Content);
                    var objectContent = m.Content as ObjectContent<IoTEdgeModule>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<IoTEdgeModule>(objectContent.Value);
                    var edgeModule = objectContent.Value as IoTEdgeModule;
                    Assert.IsNotNull(edgeModule);

                    Assert.AreEqual(mockIoTEdgeModule.ModuleName, edgeModule.ModuleName);

                    return true;
                })
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("form").Should().NotBeNull());

            // Act
            cut.Find("#rebootModule").Click();

            // Assert            
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
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


            _ = MockHttpClient
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
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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


            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("#connectButton"));

            var connectButton = cut.Find("#connectButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ConnectionStringDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            // Act
            connectButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var deleteButton = cut.WaitForElement("#deleteButton");

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService.Setup(c => c.Show<EdgeDeviceDeleteConfirmationDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            deleteButton.Click();

            // Assert            
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockRepository.VerifyAll();
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

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{this.mockdeviceId}")
                .RespondJson(mockIoTEdgeDevice);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var deleteButton = cut.WaitForElement("#deleteButton");

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService.Setup(c => c.Show<EdgeDeviceDeleteConfirmationDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            deleteButton.Click();

            // Assert            
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockRepository.VerifyAll();

            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/edge/devices", StringComparison.OrdinalIgnoreCase));
        }
    }
}
