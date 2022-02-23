using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Controllers.v10;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.v10.Device;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers.V10
{
    [TestFixture]
    public class DeviceTagSettingsControllerTest
    {
        private MockRepository mockRepository;

        private Mock<IDeviceTagMapper> mockDeviceTagMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTagTableClient;
        private Mock<ILogger<DeviceTagSettingsController>> mockLogger;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceTagMapper = this.mockRepository.Create<IDeviceTagMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTagTableClient = this.mockRepository.Create<TableClient>();
            this.mockLogger = this.mockRepository.Create<ILogger<DeviceTagSettingsController>>();
        }

        private DeviceTagSettingsController CreateDeviceTagSettingsController()
        {
            return new DeviceTagSettingsController(
                this.mockLogger.Object,
                this.mockDeviceTagMapper.Object,
                this.mockTableClientFactory.Object
                );
        }

        private TableEntity SetupMockEntity()
        {
            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            var tagName = "test";
            var entity = new TableEntity(DeviceTagSettingsController.DefaultPartitionKey, tagName);

            this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(mockDeviceTagTableClient.Object);

            return entity;
        }

        [Test]
        public async Task Post_Should_Create_New_Entity()
        {
            // Arrange
            var deviceTagSettingsController = this.CreateDeviceTagSettingsController();
            var entity = this.SetupMockEntity();

            DeviceTag tag = new DeviceTag
            {
                Name = "test",
                Label = "test",
                Required = true,
                Searchable = true
            };

            var mockResponse = this.mockRepository.Create<Response>();

            this.mockDeviceTagMapper.Setup(c => c.UpdateTableEntity(
               It.Is<TableEntity>(x => x.RowKey == tag.Name && x.PartitionKey == DeviceTagSettingsController.DefaultPartitionKey),
               It.Is<DeviceTag>(x => x == tag)));

            this.mockDeviceTagTableClient.Setup(c => c.UpsertEntityAsync(
                It.Is<TableEntity>(x => x.RowKey == tag.Name && x.PartitionKey == DeviceTagSettingsController.DefaultPartitionKey),
                It.IsAny<TableUpdateMode>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await deviceTagSettingsController.Post(new List<DeviceTag>(new[] { tag }));

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkResult>(result);

            this.mockTableClientFactory.Verify(c => c.GetDeviceTagSettings());

            this.mockDeviceTagMapper.VerifyAll();

            this.mockDeviceTagTableClient.Verify(c => c.UpsertEntityAsync(
                It.Is<TableEntity>(x => x.RowKey == tag.Name && x.PartitionKey == DeviceTagSettingsController.DefaultPartitionKey),
                It.IsAny<TableUpdateMode>(),
                It.IsAny<CancellationToken>()), Times.Once());

            this.mockRepository.VerifyAll();
        }
    }
}
