// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using FluentAssertions;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models.v10;
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
        private Mock<IDeviceModelPropertiesRepository> mockDeviceModelPropertiesRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceModelPropertiesController>>();
            this.mockMapper = this.mockRepository.Create<IMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockDeviceModelPropertiesRepository = this.mockRepository.Create<IDeviceModelPropertiesRepository>();
            this.mockUnitOfWork = this.mockRepository.Create<IUnitOfWork>();
        }

        [Test]
        public async Task GetPropertiesStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();
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

            _ = this.mockMapper.Setup(c => c.Map<DeviceProperty>(It.Is<DeviceModelProperty>(x => x.ModelId == entity.RowKey)))
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
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();
            SetupNotFoundEntity();

            // Act
            var response = await deviceModelPropertiesController.GetProperties(Guid.NewGuid().ToString());

            // Assert
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SetPropertiesShouldThrowProblemDetailsExceptionWhenModelIsNotValid()
        {
            // Arrange
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();
            var entity = SetupMockEntity();

            var properties = new[]
            {
                new DeviceProperty()
            };

            deviceModelPropertiesController.ModelState.AddModelError("Key", "Device model is invalid");

            // Act
            var act = () => deviceModelPropertiesController.SetProperties(entity.RowKey, properties);

            // Assert
            _ = await act.Should().ThrowAsync<ProblemDetailsException>();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceModelNotExistsSetPropertiesShouldReturnHttp404()
        {
            // Arrange
            var deviceModelPropertiesController = CreateDeviceModelPropertiesController();
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

        private DeviceModelPropertiesController CreateDeviceModelPropertiesController()
        {
            return new DeviceModelPropertiesController(
                this.mockUnitOfWork.Object,
                this.mockLogger.Object,
                this.mockMapper.Object,
                this.mockDeviceModelPropertiesRepository.Object,
                this.mockTableClientFactory.Object);
        }
    }
}
