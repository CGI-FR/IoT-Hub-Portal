using AzureIoTHub.Portal.Server.Factories;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using AzureIoTHub.Portal.Server.Extensions;
using Microsoft.Azure.Devices.Shared;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
using AzureIoTHub.Portal.Server.Helpers;

namespace AzureIoTHub.Portal.Server.Tests.Mappers
{
    [TestFixture]
    public class DeviceTwinMapperTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;
        private Mock<IDeviceModelCommandsManager> mockDeviceModelCommandsManager;
        private Mock<ITableClientFactory> mockTableClientFactory;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
            this.mockDeviceModelCommandsManager = this.mockRepository.Create<IDeviceModelCommandsManager>();
            this.mockTableClientFactory = this.mockRepository.Create<ITableClientFactory>();
        }

        private DeviceTwinMapper CreateDeviceTwinMapper()
        {
            return new DeviceTwinMapper(
                this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void CreateDeviceDetails_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceTwinMapper = this.CreateDeviceTwinMapper();
            Twin twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()] = "000-000-001";

            Dictionary<string, string> customTags = new()
            {
                { "assetId", Guid.NewGuid().ToString() },
                { "locationCode", Guid.NewGuid().ToString() }
            };

            foreach (KeyValuePair<string, string> customTag in customTags)
            {
                twin.Tags[customTag.Key.ToCamelCase()] = customTag.Value;
            }


            twin.Properties.Reported["DevAddr"] = Guid.NewGuid().ToString();

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns("http://fake.local/000-000-001")
                .Verifiable();

            // Act
            var result = deviceTwinMapper.CreateDeviceDetails(twin,customTags.Keys);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()].ToString(), result.ModelId);

            foreach (string tagName in customTags.Keys)
            {
                Assert.AreEqual(twin.Tags[tagName.ToCamelCase()].ToString(), result.CustomTags[tagName]);
            }

            Assert.AreEqual("http://fake.local/000-000-001", result.ImageUrl);
            Assert.AreEqual(DateTime.MinValue, result.StatusUpdatedTime);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void CreateDeviceListItem_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceTwinMapper = this.CreateDeviceTwinMapper();

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns("http://fake.local/000-000-001")
                .Verifiable();

            Twin twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()] = "000-000-001";
            twin.Tags[nameof(DeviceDetails.LocationCode).ToCamelCase()] = Guid.NewGuid().ToString();

            // Act
            var result = deviceTwinMapper.CreateDeviceListItem(twin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(twin.DeviceId, result.DeviceID);
            Assert.AreEqual("http://fake.local/000-000-001", result.ImageUrl);
            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(DateTime.MinValue, result.StatusUpdatedTime);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTwin_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var deviceTwinMapper = this.CreateDeviceTwinMapper();
            Twin twin = new Twin();
            DeviceDetails item = new DeviceDetails
            {
                LocationCode = Guid.NewGuid().ToString(),
                AssetId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString()
            };

            DeviceHelper.SetTagValue(twin, nameof(item.LocationCode), item.LocationCode);
            DeviceHelper.SetTagValue(twin, nameof(item.AssetId), item.AssetId);
            DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);

            // Act
            deviceTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.LocationCode, twin.Tags[nameof(DeviceDetails.LocationCode).ToCamelCase()].ToString());
            Assert.AreEqual(item.AssetId, twin.Tags[nameof(DeviceDetails.AssetId).ToCamelCase()].ToString());
            Assert.AreEqual(item.ModelId, twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()].ToString());

            this.mockRepository.VerifyAll();
        }
    }
}
