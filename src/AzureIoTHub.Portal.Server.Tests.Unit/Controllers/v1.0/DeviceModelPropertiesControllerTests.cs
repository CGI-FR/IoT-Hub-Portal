// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Server.Entities;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceModelPropertiesControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<DeviceModelPropertiesController>> mockLogger;
        private Mock<IMapper> mockMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTemplatesTableClient;
        private Mock<TableClient> mockDeviceModelPropertiesTableClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceModelPropertiesController>>();
            this.mockMapper = this.mockRepository.Create<IMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockDeviceModelPropertiesTableClient = this.mockRepository.Create<TableClient>();

        }

        private DeviceModelPropertiesController CreateDeviceModelPropertiesController()
        {
            return new DeviceModelPropertiesController(
                this.mockLogger.Object,
                this.mockMapper.Object,
                this.mockTableClientFactory.Object);
        }

        [Test]
        public async Task GetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelPropertiesController = this.CreateDeviceModelPropertiesController();
            var entity = SetupMockEntity();
            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.QueryAsync<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{ entity.RowKey }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(AsyncPageable<DeviceModelProperty>.FromPages(new[]
                    {
                        Page<DeviceModelProperty>.FromValues(new[]
                        {
                            new DeviceModelProperty
                            {
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = entity.RowKey
                            }
                        }, null, mockResponse.Object)
                    }));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            _ = this.mockMapper.Setup(c => c.Map<DeviceProperty>(It.Is<DeviceModelProperty>(x => x.PartitionKey == entity.RowKey)))
                .Returns((DeviceModelProperty x) => new DeviceProperty
                {
                    Name = x.Name,
                });

            // Act
            var response = await deviceModelPropertiesController.GetProperties(entity.RowKey);

            // Assert
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okObjectResult = response.Result as ObjectResult;

            Assert.NotNull(okObjectResult);
            Assert.IsAssignableFrom<List<DeviceProperty>>(okObjectResult.Value);
            var result = (List<DeviceProperty>)okObjectResult.Value;

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);


            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceModelNotExistsGetPropertiesShouldReturnHttp404()
        {
            // Arrange
            var deviceModelPropertiesController = this.CreateDeviceModelPropertiesController();
            SetupNotFoundEntity();

            // Act
            var response = await deviceModelPropertiesController.GetProperties(Guid.NewGuid().ToString());

            // Assert
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelPropertiesController = this.CreateDeviceModelPropertiesController();
            var entity = SetupMockEntity();
            var mockResponse = this.mockRepository.Create<Response>();
            var existingProperty = Guid.NewGuid().ToString();

            var properties = new[]
            {
                new DeviceProperty
                {
                    DisplayName =Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                }
            };

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.QueryAsync<DeviceModelProperty>(
                    It.Is<string>(x => x == $"PartitionKey eq '{ entity.RowKey }'"),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(AsyncPageable<DeviceModelProperty>.FromPages(new[]
                    {
                        Page<DeviceModelProperty>.FromValues(new DeviceModelProperty[]
                        {
                            new DeviceModelProperty
                            {
                                RowKey = existingProperty,
                                PartitionKey = entity.RowKey,
                            }
                        }, null, mockResponse.Object)
                    }));

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.AddEntityAsync(
                    It.Is<DeviceModelProperty>(x => x.PartitionKey == entity.RowKey),
                    It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockResponse.Object);

            _ = this.mockDeviceModelPropertiesTableClient.Setup(c => c.DeleteEntityAsync(
                It.Is<string>(x => x == entity.RowKey),
                It.Is<string>(x => x == existingProperty),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockMapper.Setup(c => c.Map(
                It.IsAny<DeviceProperty>(),
                It.IsAny<Action<IMappingOperationOptions<object, DeviceModelProperty>>>()))
                .Returns((DeviceProperty x, Action<IMappingOperationOptions<object, DeviceModelProperty>> a) => new DeviceModelProperty
                {
                    Name = x.Name,
                    PartitionKey = entity.RowKey
                });

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplateProperties())
                .Returns(this.mockDeviceModelPropertiesTableClient.Object);

            // Act
            var result = await deviceModelPropertiesController.SetProperties(entity.RowKey, properties);

            // Assert
            Assert.IsAssignableFrom<OkResult>(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceModelNotExistsSetPropertiesShouldReturnHttp404()
        {
            // Arrange
            var deviceModelPropertiesController = this.CreateDeviceModelPropertiesController();
            SetupNotFoundEntity();

            // Act
            var result = await deviceModelPropertiesController.SetProperties(Guid.NewGuid().ToString(), null);

            // Assert
            Assert.IsAssignableFrom<NotFoundResult>(result);
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
                .Returns(mockDeviceTemplatesTableClient.Object);

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
                .Returns(mockDeviceTemplatesTableClient.Object);
        }
    }
}
