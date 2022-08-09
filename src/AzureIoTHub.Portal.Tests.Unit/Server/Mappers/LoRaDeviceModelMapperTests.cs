// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Mappers
{
    using System;
    using Azure.Data.Tables;
    using Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LoRaDeviceModelMapperTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelCommandsManager> mockDeviceModelCommandsManager;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelCommandsManager = this.mockRepository.Create<IDeviceModelCommandsManager>();
            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
        }

        private LoRaDeviceModelMapper CreateLoRaDeviceModelMapper()
        {
            return new LoRaDeviceModelMapper(
                this.mockDeviceModelCommandsManager.Object,
                this.mockDeviceModelImageManager.Object);
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void CreateDeviceModelListItemStateUnderTestExpectedBehavior(bool isBuiltin, bool supportLora)
        {
            // Arrange
            var loRaDeviceModelMapper = CreateLoRaDeviceModelMapper();
            var entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var expectedModelImageUri = new Uri($"https://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedModelImageUri);

            entity[nameof(LoRaDeviceModel.IsBuiltin)] = isBuiltin;
            entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)] = supportLora;
            entity[nameof(LoRaDeviceModel.Name)] = "FAKE DEVICE";
            entity[nameof(LoRaDeviceModel.Description)] = "FAKE DESCRIPTION";

            // Act
            var result = loRaDeviceModelMapper.CreateDeviceModelListItem(entity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.RowKey, result.ModelId);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.Name)], result.Name);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.Description)], result.Description);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)], supportLora);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.IsBuiltin)], isBuiltin);

            this.mockRepository.VerifyAll();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void CreateDeviceModelStateUnderTestExpectedBehavior(bool isBuiltin, bool supportLora)
        {
            // Arrange
            var loRaDeviceModelMapper = CreateLoRaDeviceModelMapper();
            var entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var expectedModelImageUri = new Uri($"https://fake.local/{entity.RowKey}");

            _ = this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedModelImageUri);

            entity[nameof(LoRaDeviceModel.IsBuiltin)] = isBuiltin;
            entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)] = supportLora;
            entity[nameof(LoRaDeviceModel.Name)] = "FAKE DEVICE";
            entity[nameof(LoRaDeviceModel.Description)] = "FAKE DESCRIPTION";
            entity[nameof(LoRaDeviceModel.SensorDecoder)] = "FAKE SENSORDECODERURL";

            // Act
            var result = loRaDeviceModelMapper.CreateDeviceModel(
                entity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.RowKey, result.ModelId);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.Name)], result.Name);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.Description)], result.Description);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.SensorDecoder)], result.SensorDecoder);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)], supportLora);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.IsBuiltin)], isBuiltin);
            this.mockRepository.VerifyAll();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void UpdateTableEntityStateUnderTestExpectedBehavior(bool isBuiltin, bool supportLora)
        {
            // Arrange
            var loRaDeviceModelMapper = CreateLoRaDeviceModelMapper();
            var entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var model = new LoRaDeviceModel
            {
                ModelId = entity.RowKey,
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ImageUrl = new Uri("http://fake.local"),
                SensorDecoder = Guid.NewGuid().ToString(),
                IsBuiltin = isBuiltin,
                SupportLoRaFeatures = supportLora,
                UseOTAA = true
            };

            // Act
            _ = loRaDeviceModelMapper.UpdateTableEntity(
                entity,
                model);

            // Assert
            Assert.AreEqual(model.ModelId, entity.RowKey);
            Assert.AreEqual(model.Name, entity[nameof(LoRaDeviceModel.Name)]);
            Assert.AreEqual(model.Description, entity[nameof(LoRaDeviceModel.Description)]);
            Assert.AreEqual(supportLora, entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)]);
            Assert.AreEqual(isBuiltin, entity[nameof(LoRaDeviceModel.IsBuiltin)]);
            Assert.AreEqual(model.SensorDecoder, entity[nameof(LoRaDeviceModel.SensorDecoder)]);
            this.mockRepository.VerifyAll();
        }
    }
}
