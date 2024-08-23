// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Crosscutting.Extensions;
    using IoTHub.Portal.Infrastructure.Mappers;
    using Microsoft.Azure.Devices.Shared;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    [TestFixture]
    public class DeviceTwinMapperTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
        }

        private DeviceTwinMapper CreateDeviceTwinMapper()
        {
            return new DeviceTwinMapper(
                this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void CreateDeviceDetailsStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceTwinMapper = CreateDeviceTwinMapper();
            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()] = "000-000-001";

            twin.Tags["assetId"] = Guid.NewGuid().ToString();
            twin.Tags["locationCode"] = Guid.NewGuid().ToString();
            var tagsNames = new List<string>() { "assetId", "locationCode" };

            twin.Properties.Reported["DevAddr"] = Guid.NewGuid().ToString();

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns(new Uri("http://fake.local/000-000-001"))
                .Verifiable();

            // Act
            var result = deviceTwinMapper.CreateDeviceDetails(twin, tagsNames);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()].ToString(), result.ModelId);

            foreach (var tagName in tagsNames)
            {
                Assert.AreEqual(twin.Tags[tagName.ToCamelCase()].ToString(), result.Tags[tagName]);
            }

            Assert.AreEqual("http://fake.local/000-000-001", result.ImageUrl.ToString());
            Assert.AreEqual(DateTime.MinValue, result.StatusUpdatedTime);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void CreateDeviceDetailsNullTagListExpectedBehavior()
        {
            // Arrange
            var deviceTwinMapper = CreateDeviceTwinMapper();
            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()] = "000-000-001";

            List<string> tagsNames = null;

            twin.Properties.Reported["DevAddr"] = Guid.NewGuid().ToString();

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns(new Uri("http://fake.local/000-000-001"))
                .Verifiable();

            // Act
            var result = deviceTwinMapper.CreateDeviceDetails(twin, tagsNames);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()].ToString(), result.ModelId);

            Assert.IsEmpty(result.Tags);

            Assert.AreEqual("http://fake.local/000-000-001", result.ImageUrl.ToString());
            Assert.AreEqual(DateTime.MinValue, result.StatusUpdatedTime);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void CreateDeviceListItemStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceTwinMapper = CreateDeviceTwinMapper();

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns(new Uri("http://fake.local/000-000-001"))
                .Verifiable();

            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()] = "000-000-001";

            // Act
            var result = deviceTwinMapper.CreateDeviceListItem(twin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(twin.DeviceId, result.DeviceID);
            Assert.AreEqual("http://fake.local/000-000-001", result.ImageUrl.ToString());
            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(DateTime.MinValue, result.StatusUpdatedTime);

            this.mockRepository.VerifyAll();
        }

        public void CreateDeviceListItemNullTagListExpectedBehavior()
        {
            // Arrange
            var deviceTwinMapper = CreateDeviceTwinMapper();

            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()] = "000-000-001";

            List<string> tagsNames = null;

            twin.Properties.Reported["DevAddr"] = Guid.NewGuid().ToString();

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns(new Uri("http://fake.local/000-000-001"))
                .Verifiable();

            // Act
            var result = deviceTwinMapper.CreateDeviceDetails(twin, tagsNames);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()].ToString(), result.ModelId);

            Assert.IsEmpty(result.Tags);

            Assert.AreEqual("http://fake.local/000-000-001", result.ImageUrl);
            Assert.AreEqual(DateTime.MinValue, result.StatusUpdatedTime);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTwinStateUnderTestExpectedBehavior()
        {
            // Arrange
            var deviceTwinMapper = CreateDeviceTwinMapper();
            var twin = new Twin();
            var item = new DeviceDetails
            {
                ModelId = Guid.NewGuid().ToString()
            };

            item.Tags.Add("assetId", Guid.NewGuid().ToString());
            item.Tags.Add("locationCode", Guid.NewGuid().ToString());

            DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);

            twin.Tags["assetId"] = Guid.NewGuid().ToString();
            twin.Tags["locationCode"] = Guid.NewGuid().ToString();
            var tagsNames = new List<string>() { "assetId", "locationCode" };

            // Act
            deviceTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.ModelId, twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()].ToString());

            foreach (var tagName in tagsNames)
            {
                Assert.AreEqual(item.Tags[tagName], DeviceHelper.RetrieveTagValue(twin, tagName));
            }

            this.mockRepository.VerifyAll();
        }
    }
}
