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

            _ = this.mockEdgeDeviceTemplatesTableClient.Setup(c => c.Query<TableEntity>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
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
    }
}
