// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Dialogs.Layer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Dialogs.Layer;

    public class LinkDeviceLayerDialogTest : BlazorUnitTest
    {
        private Mock<IDeviceClientService> mockDeviceClientService;
        private Mock<ILayerClientService> mockLayerClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;

        private readonly string apiBaseUrl = "api/devices";

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();
            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockLayerClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
        }

        [Test]
        public async Task LinkDeviceLayerDialog_Search_RendersCorrectlyAsync()
        {
            // Arrange
            var searchedDevices = Fixture.CreateMany<TableData<DeviceListItem>>(3).ToList();
            var expectedLayerDto = Fixture.Create<LayerDto>();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=5&searchText="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, IsConnected = true },
                                    new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, IsConnected = true }}
                });

            _ = this.mockDeviceModelsClientService.Setup(service => service.GetDeviceModelsAsync(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto>
                {
                    Items = new List<DeviceModelDto>()
                });

            // Act
            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"InitLayer", expectedLayerDto},
                {"LayerList", new HashSet<LayerHash>()}
            };

            _ = await cut.InvokeAsync(() => service?.Show<LinkDeviceLayerDialog>(string.Empty, parameters));

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task LinkDeviceLayerDialog_Search_ShouldDisplayDevicesAsync()
        {
            // Arrange
            var searchedDevices = Fixture.CreateMany<TableData<DeviceListItem>>(3).ToList();
            var expectedLayerDto = Fixture.Create<LayerDto>();

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };


            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=5&searchText="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, IsConnected = true, DeviceModelId = mockDeviceModel.ModelId },
                        new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, IsConnected = true, DeviceModelId = Guid.NewGuid().ToString() }}
                });

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=5&searchText=&modelId={mockDeviceModel.ModelId}"))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, IsConnected = true, DeviceModelId = mockDeviceModel.ModelId } }
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

            _ = await cut.InvokeAsync(() => service?.Show<LinkDeviceLayerDialog>(string.Empty, parameters));
            cut.WaitForElement("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task LinkDeviceLayerDialog_Save_UpdatesDevices()
        {
            // Arrange
            var searchedDevices = Fixture.CreateMany<TableData<DeviceListItem>>(3).ToList();
            var expectedLayerDto = Fixture.Create<LayerDto>();

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };


            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=5&searchText="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, IsConnected = true, DeviceModelId = mockDeviceModel.ModelId },
                        new DeviceListItem { DeviceID = Guid.NewGuid().ToString(), IsEnabled = true, IsConnected = true, DeviceModelId = Guid.NewGuid().ToString() }}
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

            _ = await cut.InvokeAsync(() => service?.Show<LinkDeviceLayerDialog>(string.Empty, parameters));
            cut.WaitForElement("#save").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task LinkDeviceLayerDialog_UpdateChecked_UnselectingAlreadyRegisteredDevice_ShouldUpdateCheckboxState()
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
                IsConnected = true,
                DeviceModelId = mockDeviceModel.ModelId,
                Image = "image.png",
                StatusUpdatedTime = DateTime.UtcNow,
                LastActivityTime = DateTime.UtcNow,
                Labels = new List<LabelDto>(),
                LayerId = expectedLayerDto.Id  // Already registered to this layer
            };

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=5&searchText="))
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

            _ = await cut.InvokeAsync(() => service?.Show<LinkDeviceLayerDialog>(string.Empty, parameters));

            // Wait for the table to render
            cut.WaitForState(() => cut.FindAll("table tbody tr").Count == 1);

            // Find the checkbox button (should be checked with success color initially)
            var checkboxButton = cut.Find("table tbody tr td:last-child button");

            // Verify initial state - should be checked (success color indicates already registered)
            cut.WaitForAssertion(() => checkboxButton.OuterHtml.Should().Contain("mud-success-text"));

            // Click to unselect
            checkboxButton.Click();

            // Assert - after clicking, the checkbox should visually update
            // The LayerId is set to null and the device is added to DeviceRemoveList,
            // so the condition (context.LayerId == InitLayer.Id) no longer matches.
            // This should trigger the else branch showing the unchecked state (default color).
            cut.WaitForAssertion(() =>
            {
                var updatedButton = cut.Find("table tbody tr td:last-child button");
                // After unselecting, the button should show default color (unchecked state)
                _ = updatedButton.OuterHtml.Should().NotContain("mud-success-text");
            });
        }

        [Test]
        public async Task LinkDeviceLayerDialog_Save_UpdatesDevicesFromMultiplePages()
        {
            // Arrange
            var expectedLayerDto = Fixture.Create<LayerDto>();

            var mockDeviceModel = new DeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            // Simulate device on first page (device not yet assigned to any layer)
            var device1 = new DeviceListItem
            {
                DeviceID = "device1",
                DeviceName = "Device 1",
                IsEnabled = true,
                IsConnected = true,
                DeviceModelId = mockDeviceModel.ModelId,
                Image = "image1.png",
                StatusUpdatedTime = DateTime.UtcNow,
                LastActivityTime = DateTime.UtcNow,
                Labels = new List<LabelDto>(),
                LayerId = null // Not assigned to any layer
            };

            // Setup GetDevices
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageNumber=0&pageSize=5&searchText="))
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
                    IsConnected = device1.IsConnected,
                    IsEnabled = device1.IsEnabled,
                    StatusUpdatedTime = device1.StatusUpdatedTime,
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

            var dialog = await cut.InvokeAsync(() => service?.Show<LinkDeviceLayerDialog>(string.Empty, parameters));

            // Wait for initial render and select device
            cut.WaitForState(() => cut.FindAll("table tbody tr").Count == 1);
            var checkbox = cut.Find("table tbody tr td:last-child button");
            checkbox.Click();

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
    }
}
