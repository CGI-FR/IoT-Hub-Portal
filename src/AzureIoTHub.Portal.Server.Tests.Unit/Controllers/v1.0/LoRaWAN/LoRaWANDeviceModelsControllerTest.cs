// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Controllers.v1._0.LoRaWAN
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Controllers.V10;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaWANDeviceModelsControllerTest
    {
        private MockRepository mockRepository;

        private Mock<ILogger<DeviceModelsControllerBase<DeviceModel, LoRaDeviceModel>>> mockLogger;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceModelCommandMapper> mockDeviceModelCommandMapper;
        private Mock<IDeviceModelMapper<DeviceModel, LoRaDeviceModel>> mockDeviceModelMapper;
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

            this.mockLogger = this.mockRepository.Create<ILogger<DeviceModelsControllerBase<DeviceModel, LoRaDeviceModel>>>();
            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceModelCommandMapper = this.mockRepository.Create<IDeviceModelCommandMapper>();
            this.mockDeviceProvisioningServiceManager = this.mockRepository.Create<IDeviceProvisioningServiceManager>();
            this.mockDeviceModelMapper = this.mockRepository.Create<IDeviceModelMapper<DeviceModel, LoRaDeviceModel>>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
            this.mockDeviceTemplatesTableClient = this.mockRepository.Create<TableClient>();
            this.mockCommandsTableClient = this.mockRepository.Create<TableClient>();
            this.mockConfigService = this.mockRepository.Create<IConfigService>();
        }

        private LoRaWANDeviceModelsController CreateDeviceModelsController()
        {
            return new LoRaWANDeviceModelsController(
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
    }
}
