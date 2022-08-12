// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Services
{
    using Azure;
    using System.Collections.Generic;
    using System;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Moq;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Models.v10;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    [TestFixture]
    public class EdgeModelServiceTest
    {
        private MockRepository mockRepository;

        private Mock<IConfigService> mockConfigService;
        private Mock<ITableClientFactory> mockTableClientFactory;
        private Mock<IEdgeDeviceModelMapper> mockEdgeModelMapper;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<TableClient> mockEdgeDeviceTemplatesTableClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfigService = this.mockRepository.Create<IConfigService>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockEdgeModelMapper = this.mockRepository.Create<IEdgeDeviceModelMapper>();
            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
            this.mockEdgeDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
        }

        public EdgeModelService CreateEdgeModelService()
        {
            return new EdgeModelService(
                this.mockConfigService.Object,
                this.mockTableClientFactory.Object,
                this.mockEdgeModelMapper.Object,
                this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void GetEdgeModelsShouldReturnAList()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var returnedIndex = 10;

            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() => returnedIndex-- > 0);

            _ = mockEnumerator.Setup(x => x.Current)
                .Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            _ = mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockEdgeModelMapper.Setup(c => c.CreateEdgeDeviceModelListItem(It.IsAny<TableEntity>()))
                .Returns((TableEntity _) => new IoTEdgeModelListItem());

            // Act
            var result = edgeModelService.GetEdgeModels();

            // Assert
            Assert.AreEqual(10, result.Count());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEdgeModelShouldReturnValue()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();
            _ = mockResponse.Setup(c => c.Value).Returns(new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                        It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                        It.Is<string>(k => k == "test"),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockConfigService
                .Setup(x => x.GetConfigModuleList(It.IsAny<string>()))
                .ReturnsAsync(new List<IoTEdgeModule>());

            _ = this.mockEdgeModelMapper
                .Setup(x => x.CreateEdgeDeviceModel(It.IsAny<TableEntity>(), It.IsAny<List<IoTEdgeModule>>()))
                .Returns(new IoTEdgeModel());

            // Act
            var result = await edgeModelService.GetEdgeModel("test");

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateEdgeModelShouldCreateEdgeModelTemplate()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();
            SetupNotFoundEntity();

            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            var mockResponse = this.mockRepository.Create<Response>();

            _ = this.mockEdgeModelMapper
                .Setup(x => x.UpdateTableEntity(It.IsAny<TableEntity>(), It.IsAny<IoTEdgeModel>()));

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.UpsertEntityAsync(
                    It.Is<TableEntity>(x => x.RowKey == edgeModel.ModelId && x.PartitionKey == EdgeModelService.DefaultPartitionKey),
                    It.IsAny<TableUpdateMode>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);

            _ = this.mockConfigService
                .Setup(x => x.RollOutEdgeModelConfiguration(It.IsAny<IoTEdgeModel>())).Returns(Task.CompletedTask);

            // Act
            await edgeModelService.CreateEdgeModel(edgeModel);

            // Assert

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateEdgeModelShouldUpdateEdgeModelTemplate()
        {
            // Arrange
            var edgeModelService = CreateEdgeModelService();

            var edgeModel = new IoTEdgeModel()
            {
                ModelId = Guid.NewGuid().ToString()
            };

            var mockResponse = this.mockRepository.Create<Response<TableEntity>>();

            _ = this.mockEdgeDeviceTemplatesTableClient
                .Setup(c => c.GetEntityAsync<TableEntity>(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);
        }

        private void SetupNotFoundEntity()
        {
            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.GetEntityAsync<TableEntity>(
                    It.Is<string>(p => p == EdgeModelService.DefaultPartitionKey),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(StatusCodes.Status404NotFound, "Not Found"));

            _ = this.mockTableClientFactory.Setup(c => c.GetEdgeDeviceTemplates())
                .Returns(this.mockEdgeDeviceTemplatesTableClient.Object);
        }
    }
}
