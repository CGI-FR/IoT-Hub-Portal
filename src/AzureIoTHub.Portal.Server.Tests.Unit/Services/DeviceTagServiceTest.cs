using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Services
{
    [TestFixture]
    public class DeviceTagServiceTest
    {
        private MockRepository mockRepository;
        private Mock<IDeviceTagMapper> mockDeviceTagMapper;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<TableClient> mockDeviceTagTableClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDeviceTagMapper = this.mockRepository.Create<IDeviceTagMapper>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTagTableClient = this.mockRepository.Create<TableClient>();
        }

        public DeviceTagService CreateDeviceTagService()
        {
            return new DeviceTagService(
                this.mockDeviceTagMapper.Object,
                this.mockTableClientFactory.Object);
        }

        [Test]
        public async Task Update_Should_Create_New_Entity()
        {
            var deviceTagService = this.CreateDeviceTagService();

            DeviceTag tag = new DeviceTag
            {
                Name = "testName",
                Label = "testLabel",
                Required = true,
                Searchable = true
            };

            var mockResponse = this.mockRepository.Create<Response>();

            this.mockDeviceTagMapper.Setup(c => c.UpdateTableEntity(
               It.Is<TableEntity>(x => x.RowKey == tag.Name && x.PartitionKey == DeviceTagService.DefaultPartitionKey),
               It.Is<DeviceTag>(x => x == tag)));

            this.mockDeviceTagTableClient.Setup(c => c.AddEntityAsync(
                It.Is<TableEntity>(x => x.PartitionKey == DeviceTagService.DefaultPartitionKey && x.RowKey == tag.Name),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(
                It.Is<string>(x => true),
                It.IsAny<int?>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
                .Returns(Pageable<TableEntity>.FromPages(new[]                {
                    Page<TableEntity>.FromValues(new[]
                    {
                        new TableEntity(DeviceTagService.DefaultPartitionKey,tag.Name)
                    }, null, mockResponse.Object)
                }));

            this.mockDeviceTagTableClient.Setup(c => c.DeleteEntityAsync(
                It.Is<string>(x => true),
                It.IsAny<string>(),
                It.IsAny<ETag>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            this.mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(mockDeviceTagTableClient.Object);

            // Act
            await deviceTagService.UpdateTags(new List<DeviceTag>(new[] { tag }));

            // Assert
            this.mockTableClientFactory.Verify(c => c.GetDeviceTagSettings());

            this.mockDeviceTagMapper.VerifyAll();

            this.mockDeviceTagTableClient.Verify(c => c.AddEntityAsync(
                It.Is<TableEntity>(x => x.RowKey == tag.Name && x.PartitionKey == DeviceTagService.DefaultPartitionKey),
                It.IsAny<CancellationToken>()), Times.Once());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetAllTags_Should_Return_A_List()
        {
            // Arrange
            var deviceTagService = this.CreateDeviceTagService();
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
                .Returns(new TableEntity(DeviceTagService.DefaultPartitionKey, "test"));

            mockTableResponse.Setup(x => x.GetEnumerator()).Returns(mockEnumerator.Object);

            mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(mockDeviceTagTableClient.Object);

            mockDeviceTagMapper.Setup(c => c.GetDeviceTag(It.IsAny<TableEntity>()))
                .Returns((TableEntity entity) => new DeviceTag
                {
                    Name = Guid.NewGuid().ToString(),
                    Label = "test",
                    Required = true,
                    Searchable = true
                });

            // Act
            var result = deviceTagService.GetAllTags();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Count());
            Assert.IsAssignableFrom<DeviceTag>(result.First());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void GetAllTagsNames_Should_Return_A_List()
        {
            // Arrange
            var deviceTagService = this.CreateDeviceTagService();
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
                .Returns(new TableEntity(DeviceTagService.DefaultPartitionKey, "test"));

            mockTableResponse.Setup(x => x.GetEnumerator()).Returns(mockEnumerator.Object);

            mockDeviceTagTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            mockTableClientFactory.Setup(c => c.GetDeviceTagSettings())
                .Returns(mockDeviceTagTableClient.Object);

            mockDeviceTagMapper.Setup(c => c.GetDeviceTag(It.IsAny<TableEntity>()))
                .Returns((TableEntity entity) => new DeviceTag
                {
                    Name = Guid.NewGuid().ToString(),
                    Label = "test",
                    Required = true,
                    Searchable = true
                });

            // Act
            var result = deviceTagService.GetAllTagsNames();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Count());
            Assert.IsAssignableFrom<string>(result.First());

            this.mockRepository.VerifyAll();
        }
    }
}
