// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceModelPropertiesServiceTests
    {
        private MockRepository mockRepository;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTemplatesTableClient;
        private Mock<IDeviceModelPropertiesRepository> mockDeviceModelPropertiesRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<ILogger<DeviceModelPropertiesService>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockDeviceModelPropertiesRepository = this.mockRepository.Create<IDeviceModelPropertiesRepository>();
            this.mockUnitOfWork = this.mockRepository.Create<IUnitOfWork>();
            this.mockLogger = this.mockRepository.Create<ILogger<DeviceModelPropertiesService>>();
        }

        [Test]
        public async Task GetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();
            var entity = SetupMockEntity();

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.GetModelProperties(entity.RowKey))
                    .ReturnsAsync(new[]
                        {
                            new DeviceModelProperty
                            {
                                Id = Guid.NewGuid().ToString(),
                                ModelId = entity.RowKey
                            }
                        });

            // Act
            var result = await instance.GetModelProperties(entity.RowKey);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenDeviceModelNotExistsGetPropertiesShouldReturnHttp404()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();
            SetupNotFoundEntity();

            // Act
            _ = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await instance.GetModelProperties(Guid.NewGuid().ToString()));
        }

        [Test]
        public async Task SetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();
            var entity = SetupMockEntity();

            var properties = new[]
            {
                new DeviceModelProperty
                {
                    DisplayName =Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.SavePropertiesForModel(entity.RowKey, It.IsAny<IEnumerable<DeviceModelProperty>>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await instance.SavePropertiesForModel(entity.RowKey, properties);

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenExceptionOccuresSavePropertiesForModelShouldThrowInternalServerErrorException()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();

            var entity = SetupMockEntity();

            _ = this.mockDeviceModelPropertiesRepository.Setup(c => c.SavePropertiesForModel(entity.RowKey, It.IsAny<IEnumerable<DeviceModelProperty>>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Throws<DbUpdateException>();

            // Act
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () => await instance.SavePropertiesForModel(entity.RowKey, Array.Empty<DeviceModelProperty>()));
        }

        [Test]
        public void WhenExceptionOccuresDuringCheckingModelExistanceShouldThrowInternalServerErrorException()
        {
            // Arrange
            var instance = CreateDeviceModelPropertiesService();

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("failed"));

            _ = this.mockLogger.Setup(c => c.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () => await instance.GetModelProperties(Guid.NewGuid().ToString()));
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () => await instance.SavePropertiesForModel(Guid.NewGuid().ToString(), Array.Empty<DeviceModelProperty>()));

            this.mockRepository.VerifyAll();
        }

        private TableEntity SetupMockEntity()
        {
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity("0", modelId);

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == "0"),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            return entity;
        }

        private void SetupNotFoundEntity()
        {
            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == "0"),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);
        }

        private DeviceModelPropertiesService CreateDeviceModelPropertiesService()
        {
            return new DeviceModelPropertiesService(
                this.mockLogger.Object,
                this.mockUnitOfWork.Object,
                this.mockTableClientFactory.Object,
                this.mockDeviceModelPropertiesRepository.Object);
        }

    }
}
