// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.EdgeDevices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;
    using AzureIoTHub.Portal.Client.Shared;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Mocks;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeDeviceDetailPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;

        private readonly string mockdeviceId = Guid.NewGuid().ToString();

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
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
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(new IoTEdgeDevice {ConnectionState = "false"});

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            _ = cut.WaitForElement("#saveButton");

            // Act
            cut.WaitForElement("#returnButton").Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/edge/devices"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldPutEdgeDeviceDetails()
        {
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = this.mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other"
            };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockEdgeDeviceClientService.Setup(service =>
                    service.UpdateDevice(It.Is<IoTEdgeDevice>(device =>
                        mockIoTEdgeDevice.DeviceId.Equals(device.DeviceId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add($"Device {this.mockdeviceId} has been successfully updated!", Severity.Success, null)).Returns((Snackbar)null);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            // Act
            cut.WaitForElement("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeDeviceDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadDevice()
        {
            // Arrange
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            _ = cut.WaitForElement("form");

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeDeviceDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnUpdateDevice()
        {
            // Arrange
            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = this.mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other"
            };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockEdgeDeviceClientService.Setup(service =>
                    service.UpdateDevice(It.Is<IoTEdgeDevice>(device =>
                        mockIoTEdgeDevice.DeviceId.Equals(device.DeviceId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

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
                DeviceId = this.mockdeviceId,
                ConnectionState = "Connected",
                Type = "Other",
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule}
            };


            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockEdgeDeviceClientService.Setup(service => service.ExecuteModuleMethod(this.mockdeviceId, It.Is<IoTEdgeModule>(module => mockIoTEdgeModule.ModuleName.Equals(module.ModuleName, StringComparison.Ordinal)), "RestartModule"))
                .ReturnsAsync(new C2Dresult()
                {
                    Payload = "ABC",
                    Status = 200
                });

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add("Command successfully executed.", Severity.Success, null)).Returns((Snackbar)null);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var rebootButton = cut.WaitForElement("#rebootModule");

            // Act
            rebootButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockEdgeDeviceClientService.Setup(service => service.ExecuteModuleMethod(this.mockdeviceId, It.Is<IoTEdgeModule>(module => mockIoTEdgeModule.ModuleName.Equals(module.ModuleName, StringComparison.Ordinal)), "RestartModule"))
                .ReturnsAsync(new C2Dresult()
                {
                    Payload = "ABC",
                    Status = 500
                });

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>())).Returns((Snackbar)null);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var rebootButton = cut.WaitForElement("#rebootModule");

            // Act
            rebootButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockEdgeDeviceClientService.Setup(service => service.ExecuteModuleMethod(this.mockdeviceId, It.Is<IoTEdgeModule>(module => mockIoTEdgeModule.ModuleName.Equals(module.ModuleName, StringComparison.Ordinal)), "RestartModule"))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            // Act
            cut.WaitForElement("#rebootModule").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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
            
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ModuleLogsDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var logsButton = cut.WaitForElement("#showLogs");

            // Act
            logsButton.Click();

            // Assert
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

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("#connectButton"));

            var connectButton = cut.Find("#connectButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ConnectionStringDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            // Act
            connectButton.Click();

            // Assert
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

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService.Setup(c => c.Show<EdgeDeviceDeleteConfirmationDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var deleteButton = cut.WaitForElement("#deleteButton");

            // Act
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService.Setup(c => c.Show<EdgeDeviceDeleteConfirmationDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var deleteButton = cut.WaitForElement("#deleteButton");

            // Act
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/edge/devices"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
