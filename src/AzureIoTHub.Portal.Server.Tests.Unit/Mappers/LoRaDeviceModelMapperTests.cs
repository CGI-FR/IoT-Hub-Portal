using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Moq;
using NUnit.Framework;
using System;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Mappers
{
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
        public void CreateDeviceModelListItem_StateUnderTest_ExpectedBehavior(bool isBuiltin, bool supportLora)
        {
            // Arrange
            var loRaDeviceModelMapper = this.CreateLoRaDeviceModelMapper();
            TableEntity entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var expectedModelImageUri = $"https://fake.local/{entity.RowKey}";

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
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
        public void CreateDeviceModel_StateUnderTest_ExpectedBehavior(bool isBuiltin, bool supportLora)
        {
            // Arrange
            var loRaDeviceModelMapper = this.CreateLoRaDeviceModelMapper();
            TableEntity entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var expectedModelImageUri = $"https://fake.local/{entity.RowKey}";

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == entity.RowKey)))
                .Returns(expectedModelImageUri);

            entity[nameof(LoRaDeviceModel.IsBuiltin)] = isBuiltin;
            entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)] = supportLora;
            entity[nameof(LoRaDeviceModel.Name)] = "FAKE DEVICE";
            entity[nameof(LoRaDeviceModel.Description)] = "FAKE DESCRIPTION";
            entity[nameof(LoRaDeviceModel.AppEUI)] = "FAKE APP EUI";
            entity[nameof(LoRaDeviceModel.SensorDecoder)] = "FAKE SENSORDECODERURL";

            // Act
            var result = loRaDeviceModelMapper.CreateDeviceModel(
                entity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.RowKey, result.ModelId);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.Name)], result.Name);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.Description)], result.Description);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.AppEUI)], result.AppEUI);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.SensorDecoder)], result.SensorDecoder);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)], supportLora);
            Assert.AreEqual(entity[nameof(LoRaDeviceModel.IsBuiltin)], isBuiltin);
            this.mockRepository.VerifyAll();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void UpdateTableEntity_StateUnderTest_ExpectedBehavior(bool isBuiltin, bool supportLora)
        {
            // Arrange
            var loRaDeviceModelMapper = this.CreateLoRaDeviceModelMapper();
            TableEntity entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            LoRaDeviceModel model = new LoRaDeviceModel
            {
                ModelId = entity.RowKey,
                Name = Guid.NewGuid().ToString(),
                AppEUI = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ImageUrl = Guid.NewGuid().ToString(),
                SensorDecoder = Guid.NewGuid().ToString(),
                IsBuiltin = isBuiltin,
                SupportLoRaFeatures = supportLora
            };

            // Act
            loRaDeviceModelMapper.UpdateTableEntity(
                entity,
                model);

            // Assert
            Assert.AreEqual(model.ModelId, entity.RowKey);
            Assert.AreEqual(model.Name, entity[nameof(LoRaDeviceModel.Name)]);
            Assert.AreEqual(model.Description, entity[nameof(LoRaDeviceModel.Description)]);
            Assert.AreEqual(supportLora, entity[nameof(LoRaDeviceModel.SupportLoRaFeatures)]);
            Assert.AreEqual(isBuiltin, entity[nameof(LoRaDeviceModel.IsBuiltin)]);
            Assert.AreEqual(model.AppEUI, entity[nameof(LoRaDeviceModel.AppEUI)]);
            Assert.AreEqual(model.SensorDecoder, entity[nameof(LoRaDeviceModel.SensorDecoder)]);
            this.mockRepository.VerifyAll();
        }
    }
}
