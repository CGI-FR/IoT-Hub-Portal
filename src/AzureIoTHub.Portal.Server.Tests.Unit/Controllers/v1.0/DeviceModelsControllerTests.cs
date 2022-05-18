// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using FluentAssertions;
    using AzureIoTHub.Portal.Server.Exceptions;

    [TestFixture]
    public class DeviceModelsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<ILogger<DeviceModelsController>> mockLogger;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<IDeviceModelMapper<DeviceModel, DeviceModel>> mockDeviceModelMapper;
        private Mock<IDeviceService> mockDeviceService;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<IDeviceProvisioningServiceManager> mockDeviceProvisioningServiceManager;
        private Mock<TableClient> mockDeviceTemplatesTableClient;
        private Mock<TableClient> mockCommandsTableClient;
        private Mock<IConfigService> mockConfigService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceModelsController>>();
            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>();
            this.mockDeviceProvisioningServiceManager = this.mockRepository.Create<IDeviceProvisioningServiceManager>();
            this.mockDeviceModelMapper = this.mockRepository.Create<IDeviceModelMapper<DeviceModel, DeviceModel>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();
            this.mockConfigService = this.mockRepository.Create<IConfigService>();
        }

        private DeviceModelsController CreateDeviceModelsController()
        {
            return new DeviceModelsController(
                this.mockLogger.Object,
                this.mockDeviceModelImageManager.Object,
                this.mockDeviceModelMapper.Object,
                this.mockDeviceService.Object,
                this.mockTableClientFactory.Object,
                this.mockDeviceProvisioningServiceManager.Object,
                this.mockConfigService.Object);
        }

        [Test]
        public void GetListShouldReturnAList()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();
            var returnedIndex = 10;

            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() => returnedIndex-- > 0);

            _ = mockEnumerator.Setup(x => x.Current)
                .Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            _ = mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            _ = this.mockDeviceModelMapper.Setup(c => c.CreateDeviceModelListItem(It.IsAny<TableEntity>()))
                .Returns((TableEntity _) => new DeviceModel());

            // Act
            var response = deviceModelsController.GetItems();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okResponse = response.Result as OkObjectResult;

            Assert.AreEqual(200, okResponse.StatusCode);

            Assert.IsNotNull(okResponse.Value);
            var result = okResponse.Value as IEnumerable<DeviceModel>;
            Assert.AreEqual(10, result?.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenQueryFailedGetListShouldThrowAnExceptionInternalServerErrorException()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException("request failed."));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            // Act
            var act = () => deviceModelsController.GetItems();

            // Assert
            _ = act.Should().Throw<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetItemShouldReturnAValue()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            _ = mockResponse.Setup(c => c.Value).Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == "test"),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            _ = this.mockDeviceModelMapper.Setup(c => c.CreateDeviceModel(It.IsAny<TableEntity>()))
                .Returns((TableEntity _) => new DeviceModel());

            // Act
            var result = await deviceModelsController.GetItem("test");

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenModelNotExistsGetItemShouldReturn404()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();

            SetupNotFoundEntity();

            // Act
            var response = await deviceModelsController.GetItem(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);

            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once);
            this.mockDeviceModelImageManager.VerifyAll();
        }

        [Test]
        public async Task GetAvatarShouldReturnTheComputedModelAvatarUri()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();
            var entity = SetupMockEntity();

            var expectedUrl = new Uri($"http://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedUrl);

            // Act
            var response = await deviceModelsController.GetAvatar(entity.RowKey);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okResponse = response.Result as OkObjectResult;

            Assert.AreEqual(200, okResponse.StatusCode);

            Assert.IsNotNull(okResponse.Value);
            Assert.AreEqual(expectedUrl, okResponse.Value.ToString());

            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once);
            this.mockDeviceModelImageManager.VerifyAll();
        }

        [Test]
        public async Task WhenModelNotExistsGetAvatarShouldReturn404()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();

            SetupNotFoundEntity();

            // Act
            var response = await deviceModelsController.GetAvatar(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ChangeAvatarShouldChangeModelImageStream()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();
            var entity = SetupMockEntity();

            var mockFile = this.mockRepository.Create<IFormFile>();
            var expectedUrl = $"http://fake.local/{entity.RowKey}";

            using var imageStream = new MemoryStream();

            _ = mockFile.Setup(c => c.OpenReadStream())
                .Returns(imageStream);

            _ = this.mockDeviceModelImageManager.Setup(c => c.ChangeDeviceModelImageAsync(
                        It.Is<string>(x => x == entity.RowKey),
                        It.Is<Stream>(x => x == imageStream)))
                    .ReturnsAsync(expectedUrl);

            // Act
            var response = await deviceModelsController.ChangeAvatar(entity.RowKey, mockFile.Object);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okResponse = response.Result as OkObjectResult;

            Assert.AreEqual(200, okResponse.StatusCode);

            Assert.IsNotNull(okResponse.Value);
            Assert.AreEqual(expectedUrl, okResponse.Value.ToString());

            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once);
            this.mockDeviceModelImageManager.VerifyAll();
        }

        [Test]
        public async Task WhenModelNotExistsChangeAvatarShouldReturn404()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();

            SetupNotFoundEntity();

            var mockFile = this.mockRepository.Create<IFormFile>();

            // Act
            var response = await deviceModelsController.ChangeAvatar(Guid.NewGuid().ToString(), mockFile.Object);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteAvatarShouldRemoveModelImage()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();
            var entity = SetupMockEntity();

            _ = this.mockDeviceModelImageManager.Setup(c => c.DeleteDeviceModelImageAsync(
                    It.Is<string>(x => x == entity.RowKey)))
                .Returns(Task.CompletedTask);

            // Act
            var result = await deviceModelsController.DeleteAvatar(entity.RowKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NoContentResult>(result);

            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once);
            this.mockDeviceModelImageManager.VerifyAll();
        }

        [Test]
        public async Task WhenModelNotExistsDeleteAvatarShouldReturn404()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();

            SetupNotFoundEntity();

            _ = this.mockRepository.Create<IFormFile>();

            // Act
            var result = await deviceModelsController.DeleteAvatar(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task PostShouldCreateANewEntity()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();
            SetupNotFoundEntity();

            var requestModel = new DeviceModel
            {
                Name = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };
            var mockEnrollmentGroup = this.mockRepository.Create<EnrollmentGroup>(string.Empty, new SymmetricKeyAttestation(string.Empty, string.Empty));

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey == requestModel.ModelId && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockDeviceModelMapper.Setup(c => c.UpdateTableEntity(
                    It.Is<TableEntity>(x => x.RowKey == requestModel.ModelId && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<DeviceModel>()))
                .Returns(new Dictionary<string, object>());

            _ = this.mockDeviceProvisioningServiceManager.Setup(c => c.CreateEnrollmentGroupFromModelAsync(
                It.IsAny<string>(),
                It.Is<string>(x => x == requestModel.Name),
                It.IsAny<TwinCollection>()))
                .ReturnsAsync(mockEnrollmentGroup.Object);

            _ = this.mockConfigService.Setup(c => c.RollOutDeviceModelConfiguration(
                It.Is<string>(x => x == requestModel.ModelId),
                It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await deviceModelsController.Post(requestModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            var okObjectResult = result as OkResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenEmptyModelIdPostShouldCreateANewEntity()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();

            var requestModel = new DeviceModel
            {
                ModelId = String.Empty,
                Name = Guid.NewGuid().ToString(),
            };

            var mockResponse = this.mockRepository.Create<Response>();
            var mockEnrollmentGroup = this.mockRepository.Create<EnrollmentGroup>(string.Empty, new SymmetricKeyAttestation(string.Empty, string.Empty));

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey != requestModel.ModelId && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockDeviceModelMapper.Setup(c => c.UpdateTableEntity(
                    It.Is<TableEntity>(x => x.RowKey != requestModel.ModelId && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<DeviceModel>()))
                .Returns(new Dictionary<string, object>());

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            _ = this.mockDeviceProvisioningServiceManager.Setup(c => c.CreateEnrollmentGroupFromModelAsync(
                It.IsAny<string>(),
                It.Is<string>(x => x == requestModel.Name),
                It.IsAny<TwinCollection>()))
                .ReturnsAsync(mockEnrollmentGroup.Object);

            _ = this.mockConfigService.Setup(c => c.RollOutDeviceModelConfiguration(
                It.Is<string>(x => x == requestModel.ModelId),
                It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await deviceModelsController.Post(requestModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            var okObjectResult = result as OkResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceModelIdExistsPostShouldReturnBadRequest()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();
            var entity = SetupMockEntity();

            var deviceModel = new DeviceModel
            {
                ModelId = entity.RowKey
            };

            // Act
            var result = await deviceModelsController.Post(deviceModel);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task PutShouldUpdateTheDeviceModel()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();

            var deviceModel = SetupMockEntity();
            var mockResponse = this.mockRepository.Create<Response>();

            var requestModel = new DeviceModel
            {
                Name = Guid.NewGuid().ToString(),
                ModelId = deviceModel.RowKey
            };

            var mockEnrollmentGroup = this.mockRepository.Create<EnrollmentGroup>(string.Empty, new SymmetricKeyAttestation(string.Empty, string.Empty));

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey == deviceModel.RowKey && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockDeviceModelMapper.Setup(c => c.UpdateTableEntity(
                    It.Is<TableEntity>(x => x.RowKey == deviceModel.RowKey && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.Is<DeviceModel>(x => x == requestModel)))
                .Returns(new Dictionary<string, object>());

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                    .Returns(this.mockDeviceTemplatesTableClient.Object);

            _ = this.mockDeviceProvisioningServiceManager.Setup(c => c.CreateEnrollmentGroupFromModelAsync(
                It.IsAny<string>(),
                It.Is<string>(x => x == requestModel.Name),
                It.IsAny<TwinCollection>()))
                .ReturnsAsync(mockEnrollmentGroup.Object);

            _ = this.mockConfigService.Setup(c => c.RollOutDeviceModelConfiguration(
                It.Is<string>(x => x == requestModel.ModelId),
                It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await deviceModelsController.Put(requestModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);
            var okObjectResult = result as OkResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeviceModelIdNotExistsPutShouldReturnBadRequest()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();
            SetupNotFoundEntity();

            var deviceModel = new DeviceModel
            {
                ModelId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await deviceModelsController.Put(deviceModel);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);
        }

        [Test]
        public async Task DeleteShouldRemoveTheEntityCommandsAndAvatar()
        {
            // Arrange
            var deviceModelsController = CreateDeviceModelsController();
            var id = Guid.NewGuid().ToString();
            var commandId = Guid.NewGuid().ToString();
            var returned = false;

            var mockModelResponse = this.mockRepository.Create<Response<TableEntity>>();

            var mockResponse = this.mockRepository.Create<Response>();
            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() =>
            {
                if (returned)
                    return false;

                returned = true;
                return true;
            });

            _ = mockEnumerator.Setup(x => x.Current)
                .Returns(new TableEntity(id, commandId));

            _ = mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.Is<string>(k => k == id),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockModelResponse.Object);

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.DeleteEntityAsync(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == id),
                        It.IsAny<ETag>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PaginationResult<Twin>
                {
                    Items = new List<Twin>()
                });

            _ = this.mockDeviceModelImageManager.Setup(c => c.DeleteDeviceModelImageAsync(It.Is<string>(x => x == id)))
                .Returns(Task.CompletedTask);

            _ = this.mockCommandsTableClient.Setup(c => c.Query(
                    It.IsAny<Expression<Func<TableEntity, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
                        It.Is<string>(p => p == id),
                        It.Is<string>(k => k == commandId),
                        It.IsAny<ETag>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            // Act
            _ = await deviceModelsController.Delete(id);

            // Assert
            this.mockRepository.VerifyAll();
        }

        private TableEntity SetupMockEntity()
        {
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var modelId = Guid.NewGuid().ToString();
            var entity = new TableEntity(LoRaWANDeviceModelsController.DefaultPartitionKey, modelId);

            mockResponse.Setup(c => c.Value)
                .Returns(entity)
                .Verifiable();

            _ = this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
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
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            _ = this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(this.mockDeviceTemplatesTableClient.Object);
        }
    }
}
