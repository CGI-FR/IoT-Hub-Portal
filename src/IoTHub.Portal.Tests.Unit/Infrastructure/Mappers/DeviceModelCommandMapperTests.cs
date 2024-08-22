// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
{
    using Azure.Data.Tables;
    using Moq;
    using NUnit.Framework;
    using IoTHub.Portal.Infrastructure.Mappers;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class DeviceModelCommandMapperTests
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private static DeviceModelCommandMapper CreateDeviceModelCommandMapper()
        {
            return new DeviceModelCommandMapper();
        }

        [Test]
        public void GetDeviceModelCommandStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelCommandMapper = CreateDeviceModelCommandMapper();
            var entity = new TableEntity
            {
                RowKey = "000-000-001"
            };

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
        public void UpdateTableEntityStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelCommandMapper = CreateDeviceModelCommandMapper();
            var entity = new TableEntity();

            var element = new DeviceModelCommandDto
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
