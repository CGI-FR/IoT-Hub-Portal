// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.EdgeDevices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.EdgeDevices;
    using IoTHub.Portal.Client.Dialogs.EdgeDevices;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using UnitTests.Mocks;
    using AutoFixture;
    using IoTHub.Portal.Shared.Constants;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class EdgeDeviceDetailPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;
        private Mock<IEdgeModelClientService> mockIEdgeModelClientService;
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;

        private readonly string mockdeviceId = Guid.NewGuid().ToString();

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();
            this.mockIEdgeModelClientService = MockRepository.Create<IEdgeModelClientService>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockIEdgeModelClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = CloudProviders.Azure });

            _ = Services.AddSingleton<IEdgeDeviceLayoutService, EdgeDeviceLayoutService>();

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = SetupOnInitialisation();

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
            var mockIoTEdgeDevice = SetupOnInitialisation();

            _ = this.mockEdgeDeviceClientService.Setup(service =>
                    service.UpdateDevice(It.Is<IoTEdgeDevice>(device =>
                        mockIoTEdgeDevice.DeviceId.Equals(device.DeviceId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add($"Device {this.mockdeviceId} has been successfully updated!\r\nPlease note that changes might take some minutes to be visible in the list...", Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var saveButton = cut.WaitForElement("#saveButton");

            var mudButtonGroup = cut.FindComponent<MudButtonGroup>();

            mudButtonGroup.Find(".mud-menu button").Click();
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(2));

            var items = popoverProvider.FindAll("div.mud-list-item");

            var actualDeviceName = cut.WaitForElement($"#{nameof(IoTEdgeDevice.DeviceName)}").GetAttribute("value");
            cut.WaitForAssertion(() => mockIoTEdgeDevice.DeviceName.Equals(actualDeviceName, StringComparison.Ordinal));

            // Click on Save
            items[0].Click();

            // Act
            saveButton.Click();

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
        public void EdgeDeviceDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadModel()
        {
            // Arrange
            var tags = new Dictionary<string, string>()
            {
                {"test01", "test" },
                {"test02", "test" }
            };

            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = this.mockdeviceId,
                ConnectionState = "Connected",
                ModelId = Guid.NewGuid().ToString(),
                Tags = tags
            };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockIEdgeModelClientService
                .Setup(service => service.GetIoTEdgeModel(It.Is<string>(x => x.Equals(mockIoTEdgeDevice.ModelId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockDeviceTagSettingsClientService
                .Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>()
                {
                    new DeviceTagDto()
                    {
                        Name = "test01",
                        Label = "test01",
                        Required = true
                    },
                    new DeviceTagDto()
                    {
                        Name = "test02",
                        Label = "test02",
                        Required = true
                    }
                });

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
            var mockIoTEdgeDevice = SetupOnInitialisation();

            _ = this.mockEdgeDeviceClientService.Setup(service =>
                    service.UpdateDevice(It.Is<IoTEdgeDevice>(device =>
                        mockIoTEdgeDevice.DeviceId.Equals(device.DeviceId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<EdgeDeviceDetailPage>(
                ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("form").Should().NotBeNull());

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDuplicateShouldDuplicateEdgeDeviceAndRedirectToCreateEdgeDevicePage()
        {
            // Arrange
            _ = SetupOnInitialisation();

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var saveButton = cut.WaitForElement("#saveButton");

            var mudButtonGroup = cut.FindComponent<MudButtonGroup>();

            mudButtonGroup.Find(".mud-menu button").Click();
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(2));

            var items = popoverProvider.FindAll("div.mud-list-item");

            // Click on Duplicate
            items[1].Click();

            // Act
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/edge/devices/new"));
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
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule},
                ModelId = Guid.NewGuid().ToString()
            };


            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockIEdgeModelClientService
                .Setup(service => service.GetIoTEdgeModel(It.Is<string>(x => x.Equals(mockIoTEdgeDevice.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(new IoTEdgeModel());

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags()).ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockEdgeDeviceClientService.Setup(service => service.ExecuteModuleMethod(this.mockdeviceId, It.Is<string>(module => mockIoTEdgeModule.ModuleName.Equals(module, StringComparison.Ordinal)), "RestartModule"))
                .ReturnsAsync(new C2Dresult()
                {
                    Payload = "ABC",
                    Status = 200
                });

            _ = this.mockSnackbarService.Setup(c => c.Add("Command successfully executed.", Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var rebootButton = cut.WaitForElement(".rebootModule");

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
                DeviceId = this.mockdeviceId,
                ConnectionState = "Connected",
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule},
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockIEdgeModelClientService
                .Setup(service => service.GetIoTEdgeModel(It.Is<string>(x => x.Equals(mockIoTEdgeDevice.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(new IoTEdgeModel());

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags()).ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockEdgeDeviceClientService.Setup(service => service.ExecuteModuleMethod(this.mockdeviceId, It.Is<string>(module => mockIoTEdgeModule.ModuleName.Equals(module, StringComparison.Ordinal)), "RestartModule"))
                .ReturnsAsync(new C2Dresult()
                {
                    Payload = "ABC",
                    Status = 500
                });

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns(value: null);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var rebootButton = cut.WaitForElement(".rebootModule");

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
                DeviceId = this.mockdeviceId,
                ConnectionState = "Connected",
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule},
                ModelId = Guid.NewGuid().ToString(),
            };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockIEdgeModelClientService
                .Setup(service => service.GetIoTEdgeModel(It.Is<string>(x => x.Equals(mockIoTEdgeDevice.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(new IoTEdgeModel());

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags()).ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockEdgeDeviceClientService.Setup(service => service.ExecuteModuleMethod(this.mockdeviceId, It.Is<string>(module => mockIoTEdgeModule.ModuleName.Equals(module, StringComparison.Ordinal)), "RestartModule"))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            // Act
            cut.WaitForElement(".rebootModule").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeDeviceDetailPageShouldDisplayLastDeploymentStatus()
        {
            // Arrange
            var mockIoTEdgeModule = new IoTEdgeModule()
            {
                ModuleName = Guid.NewGuid().ToString()
            };

            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = this.mockdeviceId,
                ConnectionState = "Connected",
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule},
                ModelId = Guid.NewGuid().ToString(),
                LastDeployment = Fixture.Create<ConfigItem>()
            };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockIEdgeModelClientService
                .Setup(service => service.GetIoTEdgeModel(It.Is<string>(x => x.Equals(mockIoTEdgeDevice.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(new IoTEdgeModel());

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags()).ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(mockIoTEdgeDevice.LastDeployment.Name, cut.WaitForElement("#lastDeploymentName").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(mockIoTEdgeDevice.LastDeployment.DateCreation.Date, DateTime.TryParse(cut.WaitForElement("#lastDeploymentDate").GetAttribute("value"), out var dateTime) ? dateTime : null));
            cut.WaitForAssertion(() => Assert.AreEqual(mockIoTEdgeDevice.LastDeployment.Status, cut.WaitForElement("#lastDeploymentStatus").GetAttribute("value")));
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
                DeviceId = this.mockdeviceId,
                ConnectionState = "Connected",
                Modules= new List<IoTEdgeModule>(){mockIoTEdgeModule},
                ModelId = Guid.NewGuid().ToString()
            };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockIEdgeModelClientService
                .Setup(service => service.GetIoTEdgeModel(It.Is<string>(x => x.Equals(mockIoTEdgeDevice.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(new IoTEdgeModel());

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags()).ReturnsAsync(new List<DeviceTagDto>());

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ModuleLogsDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));

            var logsButton = cut.WaitForElement(".showLogs");

            // Act
            logsButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnConnectShouldDisplayDeviceCredentials()
        {
            // Arrange
            _ = SetupOnInitialisation();

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
        public void ClickOnCommandModuleShouldReturn200()
        {
            // Arrange
            _ = SetupOnInitialisation();

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("#commandTest"));

            var commandButton = cut.Find("#commandTest");

            _ = this.mockEdgeDeviceClientService
                .Setup(x => x.ExecuteModuleMethod(It.Is<string>(c => c.Equals(this.mockdeviceId, StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new C2Dresult() { Status = 200 });

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            // Act
            commandButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnCommandModuleShouldReturn400()
        {
            // Arrange
            _ = SetupOnInitialisation();

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("#commandTest"));

            var commandButton = cut.Find("#commandTest");

            _ = this.mockEdgeDeviceClientService
                .Setup(x => x.ExecuteModuleMethod(It.Is<string>(c => c.Equals(this.mockdeviceId, StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new C2Dresult() { Status = 400 });

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            // Act
            commandButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnCommandModuleShouldProcessProblemDetailsExceptionWhenIssueOccurs()
        {
            // Arrange
            _ = SetupOnInitialisation();

            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("#commandTest"));

            var commandButton = cut.Find("#commandTest");

            _ = this.mockEdgeDeviceClientService
                .Setup(x => x.ExecuteModuleMethod(It.Is<string>(c => c.Equals(this.mockdeviceId, StringComparison.Ordinal)), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            commandButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            _ = SetupOnInitialisation();

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
            _ = SetupOnInitialisation();

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

        [TestCase(false)]
        [TestCase(true)]
        public void WhenModulesArePresentCommandAreEnabledIfConnected(bool enabled)
        {
            // Arrange
            _ = SetupOnInitialisation(enabled);

            // Act
            var cut = RenderComponent<EdgeDeviceDetailPage>(ComponentParameter.CreateParameter("deviceId", this.mockdeviceId));
            cut.WaitForAssertion(() => cut.Find("#commandTest"));

            var commandButton = cut.Find("#commandTest");

            // Assert
            Assert.AreEqual(enabled, !commandButton.HasAttribute("disabled"));
        }

        private IoTEdgeDevice SetupOnInitialisation(bool connected = true)
        {
            var tags = new Dictionary<string, string>()
            {
                {"test01", "test" },
                {"test02", "test" }
            };

            var mockIoTEdgeDevice = new IoTEdgeDevice()
            {
                DeviceId = this.mockdeviceId,
                DeviceName = "test",
                ConnectionState = connected ? "Connected" : "Disconnected",
                ModelId = Guid.NewGuid().ToString(),
                Tags = tags,
                Modules = new List<IoTEdgeModule>()
                {
                    new IoTEdgeModule()
                    {
                        ModuleName = "moduleTest",
                        ImageURI = Guid.NewGuid().ToString()
                    }
                }
            };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevice(this.mockdeviceId))
                .ReturnsAsync(mockIoTEdgeDevice);

            _ = this.mockIEdgeModelClientService
                .Setup(service => service.GetIoTEdgeModel(It.Is<string>(x => x.Equals(mockIoTEdgeDevice.ModelId, StringComparison.Ordinal))))
                .ReturnsAsync(new IoTEdgeModel()
                {
                    ModelId = mockIoTEdgeDevice.ModelId,
                    EdgeModules = new List<IoTEdgeModule>()
                    {
                        new IoTEdgeModule()
                        {
                            ModuleName = "moduleTest",
                            ImageURI = Guid.NewGuid().ToString(),
                            Commands = new List<IoTEdgeModuleCommand>()
                            {
                                new IoTEdgeModuleCommand(){ Name = "commandTest"}
                            }
                        }
                    }
                });

            _ = this.mockDeviceTagSettingsClientService
                .Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>()
                {
                    new DeviceTagDto()
                    {
                        Name = "test01",
                        Label = "test01",
                        Required = true
                    },
                    new DeviceTagDto()
                    {
                        Name = "test02",
                        Label = "test02",
                        Required = true
                    }
                });

            return mockIoTEdgeDevice;
        }
    }
}
