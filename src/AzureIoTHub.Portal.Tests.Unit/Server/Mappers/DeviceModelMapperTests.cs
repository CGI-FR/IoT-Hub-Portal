// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Mappers
{
    using System;
    using Azure.Data.Tables;
    using Models.v10;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Moq;
    using NUnit.Framework;

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
        public void CreateDeviceModelStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelMapper = CreateDeviceModelMapper();
            var entity = new TableEntity
            {
                RowKey = "000-000-001"
            };
            entity["Name"] = "DeviceModelName";
            entity["Description"] = "aaa";
            entity["AppEUI"] = "AppEUI";
            entity["SensorDecoderURL"] = "SensorDecoderURL";

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns(new Uri("http://fake.local/000-000-001"))
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
        public void CreateDeviceModelListItemStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelMapper = CreateDeviceModelMapper();
            var entity = new TableEntity
            {
                RowKey = "000-000-001"
            };
            entity["Name"] = "DeviceModelName";
            entity["Description"] = "aaa";
            entity["AppEUI"] = "AppEUI";
            entity["SensorDecoderURL"] = "SensorDecoderURL";

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns(new Uri("http://fake.local/000-000-001"))
                .Verifiable();

            // Act
            var result = deviceModelMapper.CreateDeviceModelListItem(entity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("000-000-001", result.ModelId);
            Assert.AreEqual("DeviceModelName", result.Name);
            Assert.AreEqual("aaa", result.Description);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTableEntityStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceModelMapper = CreateDeviceModelMapper();
            var entity = new TableEntity();
            var model = new DeviceModel
            {
                Name = "DeviceModelName",
                Description = "Description"
            };

            // Act
            _ = deviceModelMapper.UpdateTableEntity(
                entity,
                model);

            // Assert
            Assert.AreEqual("DeviceModelName", entity["Name"]);
            Assert.AreEqual("Description", entity["Description"]);

            this.mockRepository.VerifyAll();
        }
    }
}
