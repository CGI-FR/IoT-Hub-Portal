// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
{
    using System;
    using Azure.Data.Tables;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Infrastructure.Mappers;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    [TestFixture]
    public class DeviceModelMapperTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
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
        public void BuildDeviceModelDesiredPropertiesShouldReturnEmptyDesiredPropertiesForDeviceModel()
        {
            // Arrange
            var deviceModelMapper = CreateDeviceModelMapper();
            var model = new DeviceModelDto
            {
                Name = "DeviceModelName",
                Description = "Description"
            };

            // Act
            var result = deviceModelMapper.BuildDeviceModelDesiredProperties(model);

            // Assert
            _ = result.Should().BeEmpty();

            this.mockRepository.VerifyAll();
        }
    }
}
