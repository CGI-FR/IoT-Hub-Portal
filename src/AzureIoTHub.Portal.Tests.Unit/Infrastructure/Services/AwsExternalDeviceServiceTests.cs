// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using Amazon.SecretsManager;
    using AutoFixture;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Shared;
    using AzureIoTHub.Portal.Infrastructure.Services;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AwsExternalDeviceServiceTests : BackendUnitTest
    {
        private Mock<IAmazonIoT> mockAmazonIot;
        private Mock<IAmazonGreengrassV2> mockGreengrassV2;
        private Mock<IAmazonSecretsManager> mockSecretsManager;
        private Mock<ConfigHandler> mockConfigHandler;

        private IExternalDeviceService externalDeviceService;

        public override void Setup()
        {
            base.Setup();

            this.mockAmazonIot = MockRepository.Create<IAmazonIoT>();
            this.mockGreengrassV2 = MockRepository.Create<IAmazonGreengrassV2>();
            this.mockSecretsManager = MockRepository.Create<IAmazonSecretsManager>();
            this.mockConfigHandler = MockRepository.Create<ConfigHandler>();

            _ = ServiceCollection.AddSingleton(this.mockAmazonIot.Object);
            _ = ServiceCollection.AddSingleton(this.mockGreengrassV2.Object);
            _ = ServiceCollection.AddSingleton(this.mockSecretsManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigHandler.Object);
            _ = ServiceCollection.AddSingleton<IExternalDeviceService, AwsExternalDeviceService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.externalDeviceService = Services.GetRequiredService<IExternalDeviceService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateDeviceModel_NonExistingThingType_ThingTypeCreated()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            var createThingTypeResponse = new CreateThingTypeResponse
            {
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = externalDeviceModelDto.Name
            };

            _ = this.mockAmazonIot.Setup(e => e.CreateThingTypeAsync(It.Is<CreateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createThingTypeResponse);

            _ = this.mockAmazonIot.Setup(c => c.DescribeThingGroupAsync(It.Is<DescribeThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException(It.IsAny<string>()));

            _ = this.mockAmazonIot.Setup(c => c.CreateDynamicThingGroupAsync(It.Is<CreateDynamicThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)
                                                                                                                        && d.QueryString.Equals($"thingTypeName: {externalDeviceModelDto.Name}", StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateDynamicThingGroupResponse());

            // Act
            var result = await this.externalDeviceService.CreateDeviceModel(externalDeviceModelDto);

            // Assert
            _ = result.Id.Should().Be(createThingTypeResponse.ThingTypeId);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceModel_ExistingThingType_ResourceAlreadyExistsExceptionIsThrown()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.CreateThingTypeAsync(It.Is<CreateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceAlreadyExistsException(Fixture.Create<string>()));

            // Act
            var act = () => this.externalDeviceService.CreateDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<Portal.Domain.Exceptions.ResourceAlreadyExistsException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_ExistingThingType_ThingTypeDepcrecated()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.DeprecateThingTypeAsync(It.Is<DeprecateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase) && !c.UndoDeprecate), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeprecateThingTypeResponse());

            _ = this.mockAmazonIot.Setup(c => c.DeleteDynamicThingGroupAsync(It.Is<DeleteDynamicThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new DeleteDynamicThingGroupResponse());

            // Act
            await this.externalDeviceService.DeleteDeviceModel(externalDeviceModelDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_NonExistingThingType_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.DeprecateThingTypeAsync(It.Is<DeprecateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase) && !c.UndoDeprecate), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException(Fixture.Create<string>()));

            // Act
            var act = () => this.externalDeviceService.DeleteDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<Portal.Domain.Exceptions.ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceShouldDeleteAWSCoreDevice()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockGreengrassV2.Setup(c => c.DeleteCoreDeviceAsync(It.Is<DeleteCoreDeviceRequest>(r => r.CoreDeviceThingName == deviceId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Fixture.Create<DeleteCoreDeviceResponse>());

            _ = this.mockAmazonIot.Setup(c => c.DeleteThingAsync(It.Is<DeleteThingRequest>(r => r.ThingName == deviceId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Fixture.Create<DeleteThingResponse>());

            // Act
            await this.externalDeviceService.DeleteDevice(deviceId);

            // Assert
            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenThingNotExistsOrCoreNotExistsDeviceDeleteShouldPass()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockGreengrassV2.Setup(c => c.DeleteCoreDeviceAsync(It.Is<DeleteCoreDeviceRequest>(r => r.CoreDeviceThingName == deviceId), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.GreengrassV2.Model.ResourceNotFoundException(Fixture.Create<string>()));

            _ = this.mockAmazonIot.Setup(c => c.DeleteThingAsync(It.Is<DeleteThingRequest>(r => r.ThingName == deviceId), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException(Fixture.Create<string>()));

            // Act
            await this.externalDeviceService.DeleteDevice(deviceId);

            // Assert
            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceWithTwinShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.CreateDeviceWithTwin(Fixture.Create<string>(), Fixture.Create<bool>(), Fixture.Create<Twin>(), Fixture.Create<DeviceStatus>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task CreateNewTwinFromDeviceIdShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.CreateNewTwinFromDeviceId(Fixture.Create<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task ExecuteC2DMethodShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.ExecuteC2DMethod(Fixture.Create<string>(), Fixture.Create<CloudToDeviceMethod >());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task ExecuteCustomCommandC2DMethodShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.ExecuteCustomCommandC2DMethod(Fixture.Create<string>(), Fixture.Create<string>(), Fixture.Create<CloudToDeviceMethod >());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetAllDeviceShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetAllDevice();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetAllEdgeDeviceShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetAllEdgeDevice();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetAllGatewayIDShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetAllGatewayID();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetConcentratorsCountShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetConcentratorsCount();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetConnectedDevicesCountShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetConnectedDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetConnectedEdgeDevicesCountShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetConnectedEdgeDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDeviceShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDevice(It.IsAny<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDevicesCountShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDevicesToExportShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDevicesToExport();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDeviceTwinShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDeviceTwin(It.IsAny<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDeviceTwinWithEdgeHubModuleShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDeviceTwinWithEdgeHubModule(It.IsAny<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetDeviceTwinWithModuleShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetDeviceTwinWithModule(It.IsAny<string>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetEdgeDeviceLogsShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetEdgeDeviceLogs(It.IsAny<string>(), Fixture.Create<IoTEdgeModule>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task GetEdgeDevicesCountShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.GetEdgeDevicesCount();

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task RetrieveLastConfigurationShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.RetrieveLastConfiguration(Fixture.Create<Twin>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task UpdateDeviceShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.UpdateDevice(new Device());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Test]
        public async Task UpdateDeviceTwinShouldThrowNotImplementedException()
        {
            // Act
            var act = () => this.externalDeviceService.UpdateDeviceTwin(Fixture.Create<Twin>());

            // Assert
            _ = await act.Should().ThrowAsync<NotImplementedException>();
        }
    }
}
