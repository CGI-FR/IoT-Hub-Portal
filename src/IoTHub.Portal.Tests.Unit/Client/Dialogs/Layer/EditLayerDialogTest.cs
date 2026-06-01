// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Dialogs.Layer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Dialogs.Layers;

    public class EditLayerDialogTest : BlazorUnitTest
    {
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<ILayerClientService> mockLayerClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<IPlanningClientService> mockPlanningClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockPlanningClientService = MockRepository.Create<IPlanningClientService>();

            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockPlanningClientService.Object);
        }

        [Test]
        public async Task EditLayerDialog_Search_RendersCorrectlyAsync()
        {
            // Arrange
            var expectedLayerDto = Fixture.Create<LayerDto>();

            // Setup GetPlanning mock to handle any planning ID (in case the fixture creates one)
            _ = this.mockPlanningClientService.Setup(service => service.GetPlanning(It.IsAny<string>()))
                .ReturnsAsync(new PlanningDto { DeviceModelId = null });

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices("api/devices?pageSize=10000"))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, DeviceModelId = mockDeviceModel.ModelId },
                                    new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, DeviceModelId = mockDeviceModel.ModelId }}
                });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto> { mockDeviceModel }
                });

            // Act
            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"InitLayer", expectedLayerDto},
                {"LayerList", new HashSet<LayerHash>()}
            };

            _ = await cut.InvokeAsync(() => service?.Show<EditLayerDialog>(string.Empty, parameters));

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditLayerDialog_Search_ShouldDisplayDevicesAsync()
        {
            // Arrange
            var expectedLayerDto = Fixture.Create<LayerDto>();

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            // Setup GetPlanning mock to handle any planning ID (in case the fixture creates one)
            _ = this.mockPlanningClientService.Setup(service => service.GetPlanning(It.IsAny<string>()))
                .ReturnsAsync(new PlanningDto { DeviceModelId = null });

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices("api/devices?pageSize=10000"))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, DeviceModelId = mockDeviceModel.ModelId },
                        new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, DeviceModelId = mockDeviceModel.ModelId }}
                });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>
                    {
                        mockDeviceModel
                    }
                });

            // Act
            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"InitLayer", expectedLayerDto},
                {"LayerList", new HashSet<LayerHash>()}
            };

            _ = await cut.InvokeAsync(() => service?.Show<EditLayerDialog>(string.Empty, parameters));

            // Assert - both devices should be displayed (filtering is now client-side)
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditLayerDialog_Save_UpdatesDevices()
        {
            // Arrange
            var searchedDevices = Fixture.CreateMany<TableData<DeviceListItem>>(3).ToList();
            var expectedLayerDto = Fixture.Create<LayerDto>();

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            // Setup GetPlanning mock to handle any planning ID (in case the fixture creates one)
            _ = this.mockPlanningClientService.Setup(service => service.GetPlanning(It.IsAny<string>()))
                .ReturnsAsync(new PlanningDto { DeviceModelId = null });

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices("api/devices?pageSize=10000"))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, DeviceModelId = mockDeviceModel.ModelId },
                        new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, DeviceModelId = mockDeviceModel.ModelId }}
                });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>
                    {
                        mockDeviceModel
                    }
                });

            _ = this.mockLayerClientService.Setup(service => service.UpdateLayer(expectedLayerDto))
                .Returns(Task.CompletedTask);

            // Act
            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"InitLayer", expectedLayerDto},
                {"LayerList", new HashSet<LayerHash>()}
            };

            _ = await cut.InvokeAsync(() => service?.Show<EditLayerDialog>(string.Empty, parameters));
            cut.WaitForElement("#save").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task EditLayerDialog_UpdateChecked_UnselectingAlreadyRegisteredDevice_ShouldUpdateCheckboxState()
        {
            // Arrange
            var expectedLayerDto = new LayerDto
            {
                Id = "layer-123",
                Name = "Test Layer"
            };

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = "Test Model"
            };

            var alreadyRegisteredDevice = new DeviceListItem
            {
                DeviceID = "device-already-registered",
                DeviceName = "Already Registered Device",
                IsEnabled = true,
                // IsConnected removed,
                DeviceModelId = mockDeviceModel.ModelId,
                Image = "image.png",
                // StatusUpdatedTime removed,
                LastActivityTime = DateTime.UtcNow,
                Labels = new List<LabelDto>(),
                LayerId = expectedLayerDto.Id  // Already registered to this layer
            };

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices("api/devices?pageSize=10000"))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { alreadyRegisteredDevice },
                    TotalItems = 1
                });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto> { mockDeviceModel }
                });

            // Act
            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"InitLayer", expectedLayerDto},
                {"LayerList", new HashSet<LayerHash>()}
            };

            _ = await cut.InvokeAsync(() => service?.Show<EditLayerDialog>(string.Empty, parameters));

            // Wait for the table to render
            cut.WaitForState(() => cut.FindAll("table tbody tr").Count == 1);

            // Find the row checkbox (SelectColumn renders checkbox in the first column)
            var rowCheckbox = cut.Find("table tbody tr td:first-child input[type='checkbox']");

            // Verify initial state - should be checked (device is already assigned to this layer)
            cut.WaitForAssertion(() => rowCheckbox.HasAttribute("checked").Should().BeTrue());

            // Click to unselect
            rowCheckbox.Change(false);

            // Assert - after clicking, the checkbox should be unchecked
            cut.WaitForAssertion(() =>
            {
                var updatedCheckbox = cut.Find("table tbody tr td:first-child input[type='checkbox']");
                _ = updatedCheckbox.HasAttribute("checked").Should().BeFalse();
            });
        }

        [Test]
        public async Task EditLayerDialog_Save_UpdatesDevicesFromMultiplePages()
        {
            // Arrange
            var expectedLayerDto = Fixture.Create<LayerDto>();

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            // Setup GetPlanning mock to handle any planning ID (in case the fixture creates one)
            _ = this.mockPlanningClientService.Setup(service => service.GetPlanning(It.IsAny<string>()))
                .ReturnsAsync(new PlanningDto { DeviceModelId = null });

            // Simulate device on first page (device not yet assigned to any layer)
            var device1 = new DeviceListItem
            {
                DeviceID = "device1",
                DeviceName = "Device 1",
                IsEnabled = true,
                // IsConnected removed,
                DeviceModelId = mockDeviceModel.ModelId,
                Image = "image1.png",
                // StatusUpdatedTime removed,
                LastActivityTime = DateTime.UtcNow,
                Labels = new List<LabelDto>(),
                LayerId = null // Not assigned to any layer
            };

            // Setup GetDevices
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices("api/devices?pageSize=10000"))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { device1 },
                    TotalItems = 1
                });

            // Setup GetDevice to return full device details
            _ = this.mockDeviceClientService.Setup(service => service.GetDevice("device1"))
                .ReturnsAsync(new DeviceDetails
                {
                    DeviceID = device1.DeviceID,
                    DeviceName = device1.DeviceName,
                    ModelId = device1.DeviceModelId,
                    Image = device1.Image,
                    // IsConnected removed,
                    IsEnabled = device1.IsEnabled,
                    // StatusUpdatedTime removed,
                    LastActivityTime = device1.LastActivityTime,
                    Labels = device1.Labels.ToList()
                });

            // Setup UpdateDevice to track calls
            _ = this.mockDeviceClientService.Setup(service => service.UpdateDevice(It.IsAny<DeviceDetails>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto> { mockDeviceModel }
                });

            _ = this.mockLayerClientService.Setup(service => service.UpdateLayer(expectedLayerDto))
                .Returns(Task.CompletedTask);

            // Act
            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"InitLayer", expectedLayerDto},
                {"LayerList", new HashSet<LayerHash>()}
            };

            var dialog = await cut.InvokeAsync(() => service?.Show<EditLayerDialog>(string.Empty, parameters));

            // Wait for initial render and select device using MudDataGrid SelectColumn checkbox
            cut.WaitForState(() => cut.FindAll("table tbody tr").Count == 1);
            var checkbox = cut.Find("table tbody tr td:first-child input[type='checkbox']");
            checkbox.Change(true);

            // Save the changes
            cut.WaitForElement("#save").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                // Verify GetDevice was called for the device
                this.mockDeviceClientService.Verify(service => service.GetDevice("device1"), Times.Once());

                // Verify UpdateDevice was called with LayerId set
                this.mockDeviceClientService.Verify(service =>
                    service.UpdateDevice(It.Is<DeviceDetails>(d => d.DeviceID == "device1" && d.LayerId == expectedLayerDto.Id)),
                    Times.Once());
            });
        }

        [Test]
        public async Task EditLayerDialog_WithPlanningLinkedLayer_DisablesDeviceModelSelector()
        {
            // Arrange
            var planningId = Guid.NewGuid().ToString();
            var deviceModelId = Guid.NewGuid().ToString();

            var expectedLayerDto = new LayerDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Layer",
                Planning = planningId
            };

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = deviceModelId,
                Name = "Test Model"
            };

            var mockPlanning = new PlanningDto
            {
                Id = planningId,
                Name = "Test Planning",
                DeviceModelId = deviceModelId
            };

            _ = this.mockPlanningClientService.Setup(service => service.GetPlanning(planningId))
                .ReturnsAsync(mockPlanning);

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices("api/devices?pageSize=10000"))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>(),
                    TotalItems = 0
                });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto> { mockDeviceModel }
                });

            // Act
            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"InitLayer", expectedLayerDto},
                {"LayerList", new HashSet<LayerHash>()}
            };

            _ = await cut.InvokeAsync(() => service?.Show<EditLayerDialog>(string.Empty, parameters));

            // Assert - Info alert should be displayed when layer is linked to a planning with a device model
            cut.WaitForAssertion(() =>
            {
                var alerts = cut.FindComponents<MudAlert>();
                _ = alerts.Should().NotBeEmpty();
                var infoAlert = alerts.FirstOrDefault(a => a.Instance.Severity == Severity.Info);
                _ = infoAlert.Should().NotBeNull();
            });
        }

        [Test]
        public async Task EditLayerDialog_WithoutPlanningLinkedLayer_EnablesDeviceModelSelector()
        {
            // Arrange
            var expectedLayerDto = new LayerDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Layer",
                Planning = "None"  // No planning linked
            };

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = "Test Model"
            };

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices("api/devices?pageSize=10000"))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>(),
                    TotalItems = 0
                });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto> { mockDeviceModel }
                });

            // Act
            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"InitLayer", expectedLayerDto},
                {"LayerList", new HashSet<LayerHash>()}
            };

            _ = await cut.InvokeAsync(() => service?.Show<EditLayerDialog>(string.Empty, parameters));

            // Assert - Info alert should NOT be displayed when no planning is linked
            cut.WaitForAssertion(() =>
            {
                var alerts = cut.FindComponents<MudAlert>();
                var infoAlert = alerts.FirstOrDefault(a => a.Instance.Severity == Severity.Info);
                _ = infoAlert.Should().BeNull();
            });
        }
    }
}
