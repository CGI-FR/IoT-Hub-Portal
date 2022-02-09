using Azure;
using Azure.Data.Tables;
using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;

namespace AzureIoTHub.Portal.Server.Tests.Managers
{
    [TestFixture]
    public class DeviceModelCommandsManagerTests
    {
        private MockRepository mockRepository;

        private Mock<ITableClientFactory> mockTableClientFactory;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
        }

        private DeviceModelCommandsManager CreateManager()
        {
            return new DeviceModelCommandsManager(
                this.mockTableClientFactory.Object, 
                new DeviceModelCommandMapper());
        }

        [Test]
        public void RetrieveCommands_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var manager = this.CreateManager();
            string deviceModel = "aaa";
            bool resultReturned = false;

            var entityMock = new TableEntity(deviceModel, "bbbb");
            entityMock.TryAdd(nameof(Command.Frame), "ADETRDTHDFG");

            var mockTable = this.mockRepository.Create<TableClient>();
            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            mockEnumerator.Setup(x => x.MoveNext()).Returns(() =>
            {
                resultReturned = !resultReturned;
                return resultReturned;
            });

            mockEnumerator.Setup(x => x.Current).Returns(entityMock);

            mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            mockTable.Setup(x => x.Query<TableEntity>(It.Is<string>(x => x.StartsWith($"PartitionKey  eq '{deviceModel}'")),
                                                        It.IsAny<int?>(),
                                                        It.IsAny<IEnumerable<string>>(),
                                                        It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            this.mockTableClientFactory.Setup(x => x.GetDeviceCommands())
                .Returns(mockTable.Object);

            // Act
            var result = manager.RetrieveCommands(deviceModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bbbb", result[0].CommandId);
            Assert.AreEqual("ADETRDTHDFG", result[0].Frame);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void RetrieveDeviceModelCommands_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var manager = this.CreateManager();
            string deviceModel = "aaa";
            bool resultReturned = false;

            var entityMock = new TableEntity(deviceModel, "bbbb");
            entityMock.TryAdd(nameof(DeviceModelCommand.Frame), "ADETRDTHDFG");
            entityMock.TryAdd(nameof(DeviceModelCommand.Port), 125);

            var mockTable = this.mockRepository.Create<TableClient>();
            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            mockEnumerator.Setup(x => x.MoveNext()).Returns(() =>
            {
                resultReturned = !resultReturned;
                return resultReturned;
            });

            mockEnumerator.Setup(x => x.Current).Returns(entityMock);

            mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            mockTable.Setup(x => x.Query<TableEntity>(It.Is<string>(x => x.StartsWith($"PartitionKey  eq '{deviceModel}'")),
                                                        It.IsAny<int?>(),
                                                        It.IsAny<IEnumerable<string>>(),
                                                        It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            this.mockTableClientFactory.Setup(x => x.GetDeviceCommands())
                .Returns(mockTable.Object);

            // Act
            var result = manager.RetrieveDeviceModelCommands(deviceModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bbbb", result[0].Name);
            Assert.AreEqual(125, result[0].Port);
            Assert.AreEqual("ADETRDTHDFG", result[0].Frame);

            this.mockRepository.VerifyAll();
        }
    }
}
