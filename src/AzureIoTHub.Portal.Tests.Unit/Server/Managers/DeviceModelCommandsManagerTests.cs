// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Managers
{
    using System.Collections.Generic;
    using System.Threading;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Models.v10.LoRaWAN;
    using Moq;
    using NUnit.Framework;

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
        public void RetrieveCommandsStateUnderTestExpectedBehavior()
        {
            // Arrange
            var manager = CreateManager();
            const string deviceModel = "aaa";
            var resultReturned = false;

            var entityMock = new TableEntity(deviceModel, "bbbb");
            _ = entityMock.TryAdd(nameof(Command.Frame), "ADETRDTHDFG");

            var mockTable = this.mockRepository.Create<TableClient>();
            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() =>
            {
                resultReturned = !resultReturned;
                return resultReturned;
            });

            _ = mockEnumerator.Setup(x => x.Current).Returns(entityMock);

            _ = mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            _ = mockTable.Setup(x => x.Query<TableEntity>(It.Is<string>(x => x.StartsWith($"PartitionKey  eq '{deviceModel}'")),
                                                        It.IsAny<int?>(),
                                                        It.IsAny<IEnumerable<string>>(),
                                                        It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockTableClientFactory.Setup(x => x.GetDeviceCommands())
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
        public void RetrieveDeviceModelCommandsStateUnderTestExpectedBehavior()
        {
            // Arrange
            var manager = CreateManager();
            const string deviceModel = "aaa";
            var resultReturned = false;

            var entityMock = new TableEntity(deviceModel, "bbbb");
            _ = entityMock.TryAdd(nameof(DeviceModelCommandDto.Frame), "ADETRDTHDFG");
            _ = entityMock.TryAdd(nameof(DeviceModelCommandDto.Port), 125);

            var mockTable = this.mockRepository.Create<TableClient>();
            var mockTableResponse = this.mockRepository.Create<Pageable<TableEntity>>();
            var mockEnumerator = this.mockRepository.Create<IEnumerator<TableEntity>>();
            _ = mockEnumerator.Setup(x => x.Dispose()).Callback(() => { });
            _ = mockEnumerator.Setup(x => x.MoveNext()).Returns(() =>
            {
                resultReturned = !resultReturned;
                return resultReturned;
            });

            _ = mockEnumerator.Setup(x => x.Current).Returns(entityMock);

            _ = mockTableResponse.Setup(x => x.GetEnumerator())
                .Returns(mockEnumerator.Object);

            _ = mockTable.Setup(x => x.Query<TableEntity>(It.Is<string>(x => x.StartsWith($"PartitionKey  eq '{deviceModel}'")),
                                                        It.IsAny<int?>(),
                                                        It.IsAny<IEnumerable<string>>(),
                                                        It.IsAny<CancellationToken>()))
                .Returns(mockTableResponse.Object);

            _ = this.mockTableClientFactory.Setup(x => x.GetDeviceCommands())
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
