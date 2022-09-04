// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using FluentAssertions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaWANDeviceServiceTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<LoRaWANDeviceService>> mockLogger;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<IDeviceTagService> mockDeviceTagService;
        private Mock<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>> mockDeviceTwinMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<ILoraDeviceMethodManager> mockLoraDeviceMethodManager;
        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<TableClient> mockCommandsTableClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<LoRaWANDeviceService>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockDeviceTagService = this.mockRepository.Create<IDeviceTagService>();
            this.mockDeviceTwinMapper = this.mockRepository.Create<IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails>>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockLoraDeviceMethodManager = this.mockRepository.Create<ILoraDeviceMethodManager>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>();
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();

        }

        private LoRaWANDeviceService CreateLoRaWANDeviceService()
        {
            return new LoRaWANDeviceService(
                this.mockTableClientFactory.Object,
                this.mockDeviceService.Object,
                this.mockDeviceTwinMapper.Object,
                this.mockLoraDeviceMethodManager.Object,
                this.mockDeviceModelCommandMapper.Object,
                this.mockLogger.Object);
        }

        [Test]
        public async Task ExecuteCommandStateUnderTestExpectedBehavior()
        {
            // Arrange
            var loRaWANDevicesService = CreateLoRaWANDeviceService();
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            var mockResponse = this.mockRepository.Create<Response>();
            _ = this.mockDeviceModelCommandMapper
                .Setup(c => c.GetDeviceModelCommand(It.Is<TableEntity>(x => x.RowKey == commandId && x.PartitionKey == modelId)))
                .Returns(new DeviceModelCommand
                {
                    Name = commandId,
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Pageable<TableEntity>.FromPages(new[]
                    {
                                    Page<TableEntity>.FromValues(new[]
                                    {
                                        new TableEntity(modelId, commandId)
                                    }, null, mockResponse.Object)
                    }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            using var success = new HttpResponseMessage(HttpStatusCode.Accepted);

            _ = this.mockLoraDeviceMethodManager.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommand>(x => x.Name == commandId)))
                .ReturnsAsync(success);

            _ = this.mockLogger.Setup(c => c.Log(
                    It.Is<LogLevel>(x => x == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                    .ReturnsAsync(new Twin()
                    {
                        DeviceId = deviceId,
                        ModelId = modelId,
                    });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<Twin, IEnumerable<string>>((_, _) => new LoRaDeviceDetails
                    {
                        DeviceID = deviceId,
                        ModelId = modelId,
                    });

            // Act
            await loRaWANDevicesService.ExecuteLoRaWANCommand(deviceId, commandId);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ExecuteCommandShouldThrowInternalServerErrorExceptionWheIssueOccursWhenQueryingCommands()
        {
            // Arrange
            var loRaWANDevicesService= CreateLoRaWANDeviceService();
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Throws(new RequestFailedException("test"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                    .ReturnsAsync(new Twin()
                    {
                        DeviceId = deviceId,
                        ModelId = modelId,
                    });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<Twin, IEnumerable<string>>((_, _) => new LoRaDeviceDetails
                    {
                        DeviceID = deviceId,
                        ModelId = modelId,
                    });

            // Act
            var act = () => loRaWANDevicesService.ExecuteLoRaWANCommand(deviceId, commandId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ExecuteCommandShouldReturnNotFoundWhenCommandIsNotFound()
        {
            // Arrange
            var loRaWANDevicesService = CreateLoRaWANDeviceService();
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Pageable<TableEntity>.FromPages(new[]
                {
                    Page<TableEntity>.FromValues(Array.Empty<TableEntity>(), null, mockResponse.Object)
                }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(new Twin()
                {
                    DeviceId = deviceId,
                    ModelId = modelId,
                });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                .Returns<Twin, IEnumerable<string>>((_, _) => new LoRaDeviceDetails
                {
                    DeviceID = deviceId,
                    ModelId = modelId,
                });

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await loRaWANDevicesService.ExecuteLoRaWANCommand(deviceId, commandId));

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ExecuteCommandFailedShouldThrowInternalServerErrorException()
        {
            // Arrange
            var loRaWANDevicesService = CreateLoRaWANDeviceService();
            var modelId = Guid.NewGuid().ToString();
            var deviceId = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();

            var mockResponse = this.mockRepository.Create<Response>();
            _ = this.mockDeviceModelCommandMapper
                .Setup(c => c.GetDeviceModelCommand(It.Is<TableEntity>(x => x.RowKey == commandId && x.PartitionKey == modelId)))
                .Returns(new DeviceModelCommand
                {
                    Name = commandId,
                    Frame = Guid.NewGuid().ToString(),
                    Port = 125
                });

            _ = this.mockCommandsTableClient.Setup(c => c.Query<TableEntity>(
                    It.Is<string>(x => x == $"RowKey eq '{commandId}' and PartitionKey eq '{modelId}'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Pageable<TableEntity>.FromPages(new[]
                    {
                                    Page<TableEntity>.FromValues(new[]
                                    {
                                        new TableEntity(modelId, commandId)
                                    }, null, mockResponse.Object)
                    }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            using var internalServerError = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _ = this.mockLoraDeviceMethodManager.Setup(c => c.ExecuteLoRaDeviceMessage(
                It.Is<string>(x => x == deviceId),
                It.Is<DeviceModelCommand>(x => x.Name == commandId)))
                .ReturnsAsync(internalServerError);

            _ = this.mockLogger.Setup(c => c.Log(
                It.Is<LogLevel>(x => x == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _ = this.mockDeviceService.Setup(c => c.GetDeviceTwin(It.Is<string>(x => x == deviceId)))
                .ReturnsAsync(new Twin()
                {
                    DeviceId = deviceId,
                    ModelId = modelId,
                });

            _ = this.mockDeviceTwinMapper.Setup(c => c.CreateDeviceDetails(It.IsAny<Twin>(), It.IsAny<IEnumerable<string>>()))
                .Returns<Twin, IEnumerable<string>>((_, _) => new LoRaDeviceDetails
                {
                    DeviceID = deviceId,
                    ModelId = modelId,
                });

            // Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () => await loRaWANDevicesService.ExecuteLoRaWANCommand(deviceId, commandId));

            this.mockRepository.VerifyAll();
        }
    }
}
