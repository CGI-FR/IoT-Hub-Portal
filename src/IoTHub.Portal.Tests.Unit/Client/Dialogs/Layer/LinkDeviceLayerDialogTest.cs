// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Dialogs.Layer
{
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
    }
}
