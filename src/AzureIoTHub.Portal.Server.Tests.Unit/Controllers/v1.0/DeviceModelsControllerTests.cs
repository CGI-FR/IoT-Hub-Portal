using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.Azure.Devices.Shared;
using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
using AzureIoTHub.Portal.Server.Controllers.V10;
using Microsoft.Azure.Devices.Provisioning.Service;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
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
            var result = new DeviceModelsController(
                this.mockLogger.Object,
                this.mockDeviceModelImageManager.Object,
                this.mockDeviceModelMapper.Object,
                this.mockDeviceService.Object,
                this.mockTableClientFactory.Object,
                this.mockDeviceProvisioningServiceManager.Object, 
                this.mockConfigService.Object);

            return result;
        }

        [Test]
        public void GetList_Should_Return_A_List()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();
            var returnedIndex = 10;

            var mockTable = this.mockRepository.Create<TableClient>();
            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            mockEnumerator.Setup(x => x.MoveNext()).Returns(() =>
            {
                return returnedIndex-- > 0;
            });

            mockEnumerator.Setup(x => x.Current)
                .Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            this.mockDeviceTemplatesTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);

            this.mockDeviceModelMapper.Setup(c => c.CreateDeviceModelListItem(It.IsAny<TableEntity>()))
                .Returns((TableEntity entity) => new DeviceModel());

            // Act
            var response = deviceModelsController.Get();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
            var okResponse = response.Result as OkObjectResult;

            Assert.AreEqual(200, okResponse.StatusCode);

            Assert.IsNotNull(okResponse.Value);
            var result = okResponse.Value as IEnumerable<DeviceModel>;
            Assert.AreEqual(10, result.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetItem_Should_Return_A_Value()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            mockResponse.Setup(c => c.Value).Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntity<TableEntity>(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == "test"),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .Returns(mockResponse.Object);

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);

            this.mockDeviceModelMapper.Setup(c => c.CreateDeviceModel(It.IsAny<TableEntity>()))
                .Returns((TableEntity entity) => new DeviceModel());

            // Act
            var result = deviceModelsController.Get("test");

            // Assert
            Assert.IsNotNull(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void When_Model_NotExists_GetItem_Should_Return_404()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();

            SetupNotFoundEntity();

            // Act
            var response = deviceModelsController.Get(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);

            this.mockTableClientFactory.Verify(c => c.GetDeviceTemplates(), Times.Once);
            this.mockDeviceModelImageManager.VerifyAll();
        }

        [Test]
        public void GetAvatar_Should_Return_The_Computed_Model_Avatar_Uri()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();
            var entity = SetupMockEntity();

            var expectedUrl = $"http://fake.local/{entity.RowKey}";

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedUrl);

            // Act
            var response = deviceModelsController.GetAvatar(entity.RowKey);

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
        public void When_Model_NotExists_GetAvatar_Should_Return_404()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();

            SetupNotFoundEntity();

            // Act
            var response = deviceModelsController.GetAvatar(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsAssignableFrom<NotFoundResult>(response.Result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task ChangeAvatar_Should_Change_Model_Image_Stream()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();
            var entity = SetupMockEntity();

            var mockFile = this.mockRepository.Create<IFormFile>();
            var expectedUrl = $"http://fake.local/{entity.RowKey}";

            using var imageStream = new MemoryStream();

            mockFile.Setup(c => c.OpenReadStream())
                .Returns(imageStream);

            this.mockDeviceModelImageManager.Setup(c => c.ChangeDeviceModelImageAsync(
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
        public async Task When_Model_NotExists_ChangeAvatar_Should_Return_404()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();

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
        public async Task DeleteAvatar_Should_Remove_Model_Image()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();
            var entity = SetupMockEntity();

            this.mockDeviceModelImageManager.Setup(c => c.DeleteDeviceModelImageAsync(
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
        public async Task When_Model_NotExists_DeleteAvatar_Should_Return_404()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();

            SetupNotFoundEntity();

            var mockFile = this.mockRepository.Create<IFormFile>();

            // Act
            var result = await deviceModelsController.DeleteAvatar(Guid.NewGuid().ToString());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<NotFoundResult>(result);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task Post_Should_Create_A_New_Entity()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();
            SetupNotFoundEntity();

            var requestModel = new DeviceModel
            {
                Name = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };
            var mockEnrollmentGroup = this.mockRepository.Create<EnrollmentGroup>(string.Empty, new SymmetricKeyAttestation(string.Empty, string.Empty));

            var mockResponse = this.mockRepository.Create<Response>();

            this.mockDeviceTemplatesTableClient.Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey == requestModel.ModelId && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockDeviceModelMapper.Setup(c => c.UpdateTableEntity(
                    It.Is<TableEntity>(x => x.RowKey == requestModel.ModelId && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<DeviceModel>()))
                .Returns(new Dictionary<string, object>());

            this.mockDeviceProvisioningServiceManager.Setup(c => c.CreateEnrollmentGroupFormModelAsync(
                It.IsAny<string>(),
                It.Is<string>(x => x == requestModel.Name),
                It.IsAny<TwinCollection>()))
                .ReturnsAsync(mockEnrollmentGroup.Object);

            this.mockConfigService.Setup(c => c.RolloutDeviceConfiguration(
                It.Is<string>(x => x == requestModel.ModelId),
                It.Is<string>(x => x == requestModel.Name),
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
        public async Task WhenEmptyModelId_Post_Should_Create_A_New_Entity()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();

            var requestModel = new DeviceModel
            {
                ModelId = String.Empty,
                Name = Guid.NewGuid().ToString(),
            };

            var mockResponse = this.mockRepository.Create<Response>();
            var mockEnrollmentGroup = this.mockRepository.Create<EnrollmentGroup>(string.Empty, new SymmetricKeyAttestation(string.Empty, string.Empty));

            this.mockDeviceTemplatesTableClient.Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey != requestModel.ModelId && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockDeviceModelMapper.Setup(c => c.UpdateTableEntity(
                    It.Is<TableEntity>(x => x.RowKey != requestModel.ModelId && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<DeviceModel>()))
                .Returns(new Dictionary<string, object>());

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);

            this.mockDeviceProvisioningServiceManager.Setup(c => c.CreateEnrollmentGroupFormModelAsync(
                It.IsAny<string>(),
                It.Is<string>(x => x == requestModel.Name),
                It.IsAny<TwinCollection>()))
                .ReturnsAsync(mockEnrollmentGroup.Object);

            this.mockConfigService.Setup(c => c.RolloutDeviceConfiguration(
                It.Is<string>(x => x == requestModel.ModelId),
                It.Is<string>(x => x == requestModel.Name),
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
        public async Task When_DeviceModelId_Exists_Post_Should_Return_BadRequest()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();
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
        public async Task Put_Should_Update_The_Device_Model()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();

            var deviceModel = SetupMockEntity();
            var mockResponse = this.mockRepository.Create<Response>();

            var requestModel = new DeviceModel
            {
                Name = Guid.NewGuid().ToString(),
                ModelId = deviceModel.RowKey
            };

            var mockEnrollmentGroup = this.mockRepository.Create<EnrollmentGroup>(string.Empty, new SymmetricKeyAttestation(string.Empty, string.Empty));

            this.mockDeviceTemplatesTableClient.Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey == deviceModel.RowKey && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockDeviceModelMapper.Setup(c => c.UpdateTableEntity(
                    It.Is<TableEntity>(x => x.RowKey == deviceModel.RowKey && x.PartitionKey == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.Is<DeviceModel>(x => x == requestModel)))
                .Returns(new Dictionary<string, object>());

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                    .Returns(mockDeviceTemplatesTableClient.Object);

            this.mockDeviceProvisioningServiceManager.Setup(c => c.CreateEnrollmentGroupFormModelAsync(
                It.IsAny<string>(),
                It.Is<string>(x => x == requestModel.Name),
                It.IsAny<TwinCollection>()))
                .ReturnsAsync(mockEnrollmentGroup.Object);

            this.mockConfigService.Setup(c => c.RolloutDeviceConfiguration(
                It.Is<string>(x => x == requestModel.ModelId),
                It.Is<string>(x => x == requestModel.Name),
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
        public async Task When_DeviceModelId_Not_Exists_Put_Should_Return_BadRequest()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();
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
        public async Task Delete_Should_Remove_The_Entity_Commands_And_Avatar()
        {
            // Arrange
            var deviceModelsController = this.CreateDeviceModelsController();
            string id = Guid.NewGuid().ToString();
            string commandId = Guid.NewGuid().ToString();
            bool returned = false;

            var mockModelResponse = this.mockRepository.Create<Response<TableEntity>>();
            mockModelResponse.Setup(c => c.Value)
                .Returns(new TableEntity(id, LoRaWANDeviceModelsController.DefaultPartitionKey));

            var mockResponse = this.mockRepository.Create<Response>();
            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            mockEnumerator.Setup(x => x.MoveNext()).Returns(() =>
            {
                if (returned)
                    return false;

                returned = true;
                return true;
            });

            mockEnumerator.Setup(x => x.Current)
                .Returns(new TableEntity(id, commandId));

            mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntity<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.Is<string>(k => k == id),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
            .Returns(mockModelResponse.Object);

            this.mockDeviceTemplatesTableClient.Setup(c => c.DeleteEntityAsync(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == id),
                        It.IsAny<ETag>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockDeviceService.Setup(c => c.GetAllDevice(
                    It.Is<string>(x => string.IsNullOrEmpty(x)),
                    It.Is<string>(x => string.IsNullOrEmpty(x))))
                .ReturnsAsync(new List<Twin>());

            this.mockDeviceModelImageManager.Setup(c => c.DeleteDeviceModelImageAsync(It.Is<string>(x => x == id)))
                .Returns(Task.CompletedTask);

            this.mockCommandsTableClient.Setup(c => c.Query(
                    It.IsAny<Expression<Func<TableEntity, bool>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            this.mockCommandsTableClient.Setup(c => c.DeleteEntityAsync(
                        It.Is<string>(p => p == id),
                        It.Is<string>(k => k == commandId),
                        It.IsAny<ETag>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);

            this.mockTableClientFactory.Setup(c => c.GetDeviceCommands())
                .Returns(this.mockCommandsTableClient.Object);

            // Act
            var result = await deviceModelsController.Delete(id);

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

            this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntity<TableEntity>(
                        It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                        It.Is<string>(k => k == modelId),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .Returns(mockResponse.Object);

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);

            return entity;
        }

        private void SetupNotFoundEntity()
        {
            this.mockDeviceTemplatesTableClient.Setup(c => c.GetEntity<TableEntity>(
                    It.Is<string>(p => p == LoRaWANDeviceModelsController.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            this.mockTableClientFactory.Setup(c => c.GetDeviceTemplates())
                .Returns(mockDeviceTemplatesTableClient.Object);
        }
    }
}
