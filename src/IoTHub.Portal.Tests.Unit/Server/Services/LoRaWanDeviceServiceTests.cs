// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoMapper;
    using Azure.Messaging.EventHubs;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Mappers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Infrastructure;
    using IoTHub.Portal.Infrastructure.Services;
    using EntityFramework.Exceptions.Common;
    using FluentAssertions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Portal.Domain.Entities;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;
    using UnitTests.Bases;

    [TestFixture]
    public class LoRaWanDeviceServiceTests : BackendUnitTest
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<ILorawanDeviceRepository> mockLorawanDeviceRepository;
        private Mock<ILoRaDeviceTelemetryRepository> mockLoRaDeviceTelemetryRepository;
        private Mock<IDeviceTagValueRepository> mockDeviceTagValueRepository;
        private Mock<ILabelRepository> mockLabelRepository;
        private Mock<IExternalDeviceService> mockExternalDevicesService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>> mockDeviceTwinMapper;

        private IDeviceService<LoRaDeviceDetails> lorawanDeviceService;

        public override void Setup()
        {
            base.Setup();

            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockLorawanDeviceRepository = MockRepository.Create<ILorawanDeviceRepository>();
            this.mockLoRaDeviceTelemetryRepository = MockRepository.Create<ILoRaDeviceTelemetryRepository>();
            this.mockDeviceTagValueRepository = MockRepository.Create<IDeviceTagValueRepository>();
            this.mockLabelRepository = MockRepository.Create<ILabelRepository>();
            this.mockExternalDevicesService = MockRepository.Create<IExternalDeviceService>();
            this.mockDeviceTagService = MockRepository.Create<IDeviceTagService>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceTwinMapper = MockRepository.Create<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>>();

            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockLorawanDeviceRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLoRaDeviceTelemetryRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagValueRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockLabelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockExternalDevicesService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTagService.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceTwinMapper.Object);
            _ = ServiceCollection.AddSingleton(DbContext);
            _ = ServiceCollection.AddSingleton<IDeviceService<LoRaDeviceDetails>, LoRaWanDeviceService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.lorawanDeviceService = Services.GetRequiredService<IDeviceService<LoRaDeviceDetails>>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public void ServiceCanDeserializeEventAuthMethod()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            var authMethodJson = /*lang=json,strict*/ "{\"scope\":\"module\",\"type\":\"sas\",\"issuer\":\"iothub\"}";

            var eventAuthMethod = JsonSerializer.Deserialize<ConnectionAuthMethod>(authMethodJson.ToString(), options);

            Assert.AreEqual(authMethodJson, JsonSerializer.Serialize(eventAuthMethod, options));
        }

        [Test]
        public async Task GetDevice_DeviceExist_ReturnsExpectedDevice()
        {
            // Arrange
            var expectedDevice = Fixture.Create<LorawanDevice>();

            var expectedImageUri = Fixture.Create<Uri>();
            var expectedDeviceDto = Mapper.Map<LoRaDeviceDetails>(expectedDevice);
            expectedDeviceDto.ImageUrl = expectedImageUri;

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(expectedDeviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(expectedDevice);

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(It.IsAny<string>()))
                .Returns(expectedImageUri);

            _ = this.mockDeviceTagService.Setup(service => service.GetAllTagsNames())
                .Returns(expectedDevice.Tags.Select(tag => tag.Name));

            // Act
            var result = await this.lorawanDeviceService.GetDevice(expectedDeviceDto.DeviceID);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDevice_DeviceNotExist_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceId, d => d.Tags, d => d.Labels))
                .ReturnsAsync((LorawanDevice)null);

            // Act
            var act = () => this.lorawanDeviceService.GetDevice(deviceId);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDevice_NewDevice_DeviceCreated()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.CreateNewTwinFromDeviceId(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<LoRaDeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service =>
                    service.CreateDeviceWithTwin(deviceDto.DeviceID, false, It.IsAny<Twin>(), It.IsAny<DeviceStatus>()))
                .ReturnsAsync(new BulkRegistryOperationResult());

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<LorawanDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.lorawanDeviceService.CreateDevice(deviceDto);

            // Assert
            _ = result.Should().BeEquivalentTo(deviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDevice_DbUpdateExceptionIsThrown_UniqueConstraintExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.CreateNewTwinFromDeviceId(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<LoRaDeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service =>
                    service.CreateDeviceWithTwin(deviceDto.DeviceID, false, It.IsAny<Twin>(), It.IsAny<DeviceStatus>()))
                .ReturnsAsync(new BulkRegistryOperationResult());

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.InsertAsync(It.IsAny<LorawanDevice>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new UniqueConstraintException());

            // Act
            var act = () => this.lorawanDeviceService.CreateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<UniqueConstraintException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDevice_DeviceExist_DeviceUpdated()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.GetDevice(deviceDto.DeviceID))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<Microsoft.Azure.Devices.Device>()))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.GetDeviceTwin(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<LoRaDeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new LorawanDevice
                {
                    Tags = new List<DeviceTagValue>
                    {
                        new()
                        {
                            Id = Fixture.Create<string>()
                        }
                    },
                    Labels = Fixture.CreateMany<Label>(1).ToList()
                });

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLorawanDeviceRepository.Setup(repository => repository.Update(It.IsAny<LorawanDevice>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await this.lorawanDeviceService.UpdateDevice(deviceDto);

            // Assert
            _ = result.Should().BeEquivalentTo(deviceDto);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDevice_DeviceNotExist_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.GetDevice(deviceDto.DeviceID))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<Microsoft.Azure.Devices.Device>()))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.GetDeviceTwin(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<LoRaDeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync((LorawanDevice)null);

            // Act
            var act = () => this.lorawanDeviceService.UpdateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateDevice_DbUpdateExceptionIsRaised_CannotInsertNullExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.GetDevice(deviceDto.DeviceID))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDevice(It.IsAny<Microsoft.Azure.Devices.Device>()))
                .ReturnsAsync(new Microsoft.Azure.Devices.Device());

            _ = this.mockExternalDevicesService.Setup(service => service.GetDeviceTwin(deviceDto.DeviceID))
                .ReturnsAsync(new Twin());

            this.mockDeviceTwinMapper
                .Setup(mapper => mapper.UpdateTwin(It.IsAny<Twin>(), It.IsAny<LoRaDeviceDetails>()))
                .Verifiable();

            _ = this.mockExternalDevicesService.Setup(service => service.UpdateDeviceTwin(It.IsAny<Twin>()))
                .ReturnsAsync(new Twin());

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new LorawanDevice
                {
                    Tags = new List<DeviceTagValue>(),
                    Labels = new List<Label>()
                });

            this.mockLorawanDeviceRepository.Setup(repository => repository.Update(It.IsAny<LorawanDevice>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new CannotInsertNullException());

            // Act
            var act = () => this.lorawanDeviceService.UpdateDevice(deviceDto);

            // Assert
            _ = await act.Should().ThrowAsync<CannotInsertNullException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice_DeviceExist_DeviceDeleted()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.DeleteDevice(deviceDto.DeviceID))
                .Returns(Task.CompletedTask);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new LorawanDevice
                {
                    Tags = Fixture.CreateMany<DeviceTagValue>(5).ToList(),
                    Labels = Fixture.CreateMany<Label>(5).ToList()
                });

            this.mockDeviceTagValueRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLabelRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            this.mockLorawanDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceID))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.lorawanDeviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice_DeviceNotExist_NothingIsDone()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.DeleteDevice(deviceDto.DeviceID))
                .Returns(Task.CompletedTask);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync((LorawanDevice)null);

            // Act
            await this.lorawanDeviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDevice_DbUpdateExceptionIsRaised_ReferenceConstraintExceptionIsThrown()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            _ = this.mockExternalDevicesService.Setup(service => service.DeleteDevice(deviceDto.DeviceID))
                .Returns(Task.CompletedTask);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Tags, d => d.Labels))
                .ReturnsAsync(new LorawanDevice
                {
                    Tags = new List<DeviceTagValue>(),
                    Labels = new List<Label>()
                });

            this.mockLorawanDeviceRepository.Setup(repository => repository.Delete(deviceDto.DeviceID))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new ReferenceConstraintException());

            // Act
            var act = () => this.lorawanDeviceService.DeleteDevice(deviceDto.DeviceID);

            // Assert
            _ = await act.Should().ThrowAsync<ReferenceConstraintException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTelemetry_ExistingDevice_ReturnsTelemetry()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            var deviceEntity = new LorawanDevice
            {
                Id = deviceDto.DeviceID,
                Telemetry = Fixture.CreateMany<LoRaDeviceTelemetry>(1).ToList()
            };

            var expectedTelemetry = Mapper.Map<ICollection<LoRaDeviceTelemetry>, IEnumerable<LoRaDeviceTelemetryDto>>(deviceEntity.Telemetry);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Telemetry))
               .ReturnsAsync(deviceEntity);

            // Act
            var result = await this.lorawanDeviceService.GetDeviceTelemetry(deviceDto.DeviceID);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedTelemetry);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetDeviceTelemetry_NonExistingDevice_ReturnsEmptyArray()
        {
            // Arrange
            var deviceDto = new LoRaDeviceDetails
            {
                DeviceID = Fixture.Create<string>()
            };

            var expectedTelemetry = Array.Empty<LoRaDeviceTelemetryDto>();

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceDto.DeviceID, d => d.Telemetry))
               .ReturnsAsync((LorawanDevice)null);

            // Act
            var result = await this.lorawanDeviceService.GetDeviceTelemetry(deviceDto.DeviceID);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedTelemetry);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenEventDataIsNull_ShouldNotProcess()
        {
            // Arrange

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(null);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenEventDataDoesntHaveSystemProperties_ShouldNotProcess()
        {
            // Arrange
            var telemeryMessage = Fixture.Create<LoRaTelemetry>();
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(JsonSerializer.Serialize(telemeryMessage)), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber);

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }


        [Test]
        public async Task WhenEventDataIsNotFromDevice_ShouldNotProcess()
        {
            // Arrange
            var telemeryMessage = Fixture.Create<LoRaTelemetry>();
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var systemProperties = new Dictionary<string, object>
            {
                {  "iothub-connection-auth-method" , /*lang=json,strict*/ "{\"scope\":\"module\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}" }
            }.AsReadOnly();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(JsonSerializer.Serialize(telemeryMessage)), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber, systemProperties: systemProperties);

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenEventDataDoesntHaveDeviceIdInSystemProperties_ShouldNotProcess()
        {
            // Arrange
            var telemeryMessage = Fixture.Create<LoRaTelemetry>();
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var systemProperties = new Dictionary<string, object>
            {
                {  "iothub-connection-auth-method" , /*lang=json,strict*/ "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}" }
            }.AsReadOnly();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(JsonSerializer.Serialize(telemeryMessage)), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber, systemProperties: systemProperties);

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }


        [Test]
        public async Task ProcessTelemetryEvent_EventDataForExistingDevice_TelemetryIsAddedToDevice()
        {
            // Arrange
            var telemeryMessage = Fixture.Create<LoRaTelemetry>();
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var lorawanDevice = new LorawanDevice
            {
                Id = telemeryMessage.DeviceEUI
            };

            var systemProperties = new Dictionary<string, object>
            {
                { "iothub-connection-device-id", telemeryMessage.DeviceEUI },
                {  "iothub-connection-auth-method" , /*lang=json,strict*/ "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}" }
            }.AsReadOnly();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(JsonSerializer.Serialize(telemeryMessage)), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber, systemProperties: systemProperties);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(telemeryMessage.DeviceEUI, d => d.Telemetry))
               .ReturnsAsync(lorawanDevice);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ProcessTelemetryEvent_DbUpdateExceptionIsThrown_NothingIsDone()
        {
            // Arrange
            var telemeryMessage = Fixture.Create<LoRaTelemetry>();
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var lorawanDevice = new LorawanDevice
            {
                Id = telemeryMessage.DeviceEUI
            };

            var systemProperties = new Dictionary<string, object>
            {
                { "iothub-connection-device-id", telemeryMessage.DeviceEUI },
                {  "iothub-connection-auth-method" , /*lang=json,strict*/ "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}" }
            }.AsReadOnly();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(JsonSerializer.Serialize(telemeryMessage)), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber, systemProperties: systemProperties);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(telemeryMessage.DeviceEUI, d => d.Telemetry))
               .ReturnsAsync(lorawanDevice);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .ThrowsAsync(new DbUpdateException());

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ProcessTelemetryEvent_EventDataNotValid_TelemetryIsNotAddedToDevice()
        {
            // Arrange
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(Fixture.Create<string>()), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber);

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ProcessTelemetryEvent_EventDataForExistingDeviceWithSameTelemetryId_TelemetryIsNotAddedToDevice()
        {
            // Arrange
            var telemeryMessage = Fixture.Create<LoRaTelemetry>();
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var lorawanDevice = new LorawanDevice
            {
                Id = telemeryMessage.DeviceEUI,
                Telemetry = new List<LoRaDeviceTelemetry>()
                {
                    new LoRaDeviceTelemetry
                    {
                        Id= sequenceNumber.ToString(CultureInfo.InvariantCulture),
                        EnqueuedTime = enqueuedAt.DateTime,
                        Telemetry = telemeryMessage
                    }
                }
            };

            var systemProperties = new Dictionary<string, object>
            {
                { "iothub-connection-device-id", telemeryMessage.DeviceEUI },
                {  "iothub-connection-auth-method" , /*lang=json,strict*/ "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}" }
            }.AsReadOnly();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(JsonSerializer.Serialize(telemeryMessage)), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber, systemProperties: systemProperties);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(telemeryMessage.DeviceEUI, d => d.Telemetry))
               .ReturnsAsync(lorawanDevice);

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ProcessTelemetryEvent_EventDataForNonExistingDevice_TelemetryIsNotAddedToDevice()
        {
            // Arrange
            var telemeryMessage = Fixture.Create<LoRaTelemetry>();
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var systemProperties = new Dictionary<string, object>
            {
                { "iothub-connection-device-id", telemeryMessage.DeviceEUI },
                {  "iothub-connection-auth-method" , /*lang=json,strict*/ "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}" }
            }.AsReadOnly();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(JsonSerializer.Serialize(telemeryMessage)), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber, systemProperties: systemProperties);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(telemeryMessage.DeviceEUI, d => d.Telemetry))
               .ReturnsAsync((LorawanDevice)null);

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ProcessTelemetryEvent_NewEventDataForExistingDeviceWith100Telemetry_OnlyLatestHundredTelemetryMustBeKeptByDevice()
        {
            // Arrange
            var telemeryMessage = Fixture.Create<LoRaTelemetry>();
            var enqueuedAt = Fixture.Create<DateTimeOffset>();
            var sequenceNumber = Fixture.Create<long>();

            var lorawanDevice = new LorawanDevice
            {
                Id = telemeryMessage.DeviceEUI,
                Telemetry = Fixture.CreateMany<LoRaDeviceTelemetry>(100).ToList()
            };

            var systemProperties = new Dictionary<string, object>
            {
                { "iothub-connection-device-id", telemeryMessage.DeviceEUI },
                { "iothub-connection-auth-method" , /*lang=json,strict*/ "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}" }
            }.AsReadOnly();

            var eventMessage = EventHubsModelFactory.EventData(new BinaryData(JsonSerializer.Serialize(telemeryMessage)), enqueuedTime: enqueuedAt, sequenceNumber: sequenceNumber, systemProperties: systemProperties);

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(telemeryMessage.DeviceEUI, d => d.Telemetry))
               .ReturnsAsync(lorawanDevice);

            this.mockLoRaDeviceTelemetryRepository.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Verifiable();

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.lorawanDeviceService.ProcessTelemetryEvent(eventMessage);

            // Assert
            MockRepository.VerifyAll();
            this.mockUnitOfWork.Verify(work => work.SaveAsync(), Times.Exactly(1));
            _ = lorawanDevice.Telemetry.Should().HaveCount(100);
        }

        [Test]
        public async Task CheckIfDeviceExistsShouldReturnFalseIfDeviceDoesNotExist()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceId))
                .ReturnsAsync((LorawanDevice)null);

            // Act
            var result = await this.lorawanDeviceService.CheckIfDeviceExists(deviceId);

            // Assert
            Assert.IsFalse(result);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CheckIfDeviceExistsShouldReturnTrueIfDeviceExists()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = this.mockLorawanDeviceRepository.Setup(repository => repository.GetByIdAsync(deviceId))
                .ReturnsAsync(new LorawanDevice());

            // Act
            var result = await this.lorawanDeviceService.CheckIfDeviceExists(deviceId);

            // Assert
            Assert.IsTrue(result);
            MockRepository.VerifyAll();
        }
    }
}
