using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10;
using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AzureIoTHub.Portal.Server.Tests.Mappers
{
    [TestFixture]
    public class DeviceModelMapperTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceModelCommandsManager> mockDeviceModelCommandsManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();

            this.mockDeviceModelCommandsManager = this.mockRepository.Create<IDeviceModelCommandsManager>();
        }

        private DeviceModelMapper CreateDeviceModelMapper()
        {
            return new DeviceModelMapper(this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void CreateDeviceModel_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceModelMapper = this.CreateDeviceModelMapper();
            var entity = new TableEntity();

            entity.RowKey = "000-000-001";
            entity["Name"] = "DeviceModelName";
            entity["Description"] = "aaa";
            entity["AppEUI"] = "AppEUI";
            entity["SensorDecoderURL"] = "SensorDecoderURL";

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns("http://fake.local/000-000-001")
                .Verifiable();

            // Act
            var result = deviceModelMapper.CreateDeviceModel(entity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("000-000-001", result.ModelId);
            Assert.AreEqual("DeviceModelName", result.Name);
            Assert.AreEqual("aaa", result.Description);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTableEntity_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceModelMapper = this.CreateDeviceModelMapper();
            TableEntity entity = new TableEntity();
            DeviceModel model = new DeviceModel
            {
                Name = "DeviceModelName",
                Description = "Description"
            };

            // Act
            deviceModelMapper.UpdateTableEntity(
                entity,
                model);

            // Assert
            Assert.AreEqual("DeviceModelName", entity["Name"]);
            Assert.AreEqual("Description", entity["Description"]);

            this.mockRepository.VerifyAll();
        }
    }
}
