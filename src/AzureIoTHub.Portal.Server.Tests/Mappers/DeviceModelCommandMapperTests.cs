using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Moq;
using NUnit.Framework;

namespace AzureIoTHub.Portal.Server.Tests.Mappers
{
    [TestFixture]
    public class DeviceModelCommandMapperTests
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private DeviceModelCommandMapper CreateDeviceModelCommandMapper()
        {
            return new DeviceModelCommandMapper();
        }

        [Test]
        public void GetDeviceModelCommand_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceModelCommandMapper = this.CreateDeviceModelCommandMapper();
            var entity = new TableEntity();

            entity.RowKey = "000-000-001";
            entity["Frame"] = "ExpectedFrame";
            entity["Port"] = 10;

            // Act
            var result = deviceModelCommandMapper.GetDeviceModelCommand(entity);

            // Assert
            Assert.AreEqual("000-000-001", result.Name);
            Assert.AreEqual("ExpectedFrame", result.Frame);
            Assert.AreEqual(10, result.Port);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTableEntity_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceModelCommandMapper = this.CreateDeviceModelCommandMapper();
            var entity = new TableEntity();

            var element = new DeviceModelCommand
            {
                Name = "000-000-001",
                Frame = "ExpectedFrame",
                Port = 10
            };

            // Act
            deviceModelCommandMapper.UpdateTableEntity(
                entity,
                element);

            // Assert
            Assert.AreEqual("ExpectedFrame", entity["Frame"]);
            Assert.AreEqual(10, entity["Port"]);
            this.mockRepository.VerifyAll();
        }
    }
}
