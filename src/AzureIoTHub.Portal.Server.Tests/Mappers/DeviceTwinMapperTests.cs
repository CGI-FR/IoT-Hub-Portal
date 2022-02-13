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
                this.mockDeviceModelImageManager.Object,
                this.mockDeviceModelCommandsManager.Object,
                this.mockTableClientFactory.Object);
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
            twin.Tags[nameof(DeviceDetails.LocationCode).ToCamelCase()] = Guid.NewGuid().ToString();
            twin.Tags[nameof(DeviceDetails.AssetId).ToCamelCase()] = Guid.NewGuid().ToString();
            twin.Tags[nameof(DeviceDetails.DeviceType).ToCamelCase()] = Guid.NewGuid().ToString();

            twin.Properties.Reported["DevAddr"] = Guid.NewGuid().ToString();

            twin.Properties.Desired[nameof(DeviceDetails.AppEUI)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(DeviceDetails.AppKey)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(DeviceDetails.SensorDecoder)] = Guid.NewGuid().ToString();

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(c => c.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns("http://fake.local/000-000-001")
                .Verifiable();

            this.mockDeviceModelCommandsManager.Setup(c => c.RetrieveCommands(It.Is<string>(x => x.Equals("000-000-001", StringComparison.OrdinalIgnoreCase))))
                .Returns(new List<Command>
                {
                    new Command
                    {
                        Frame = "Frame",
                        CommandId = Guid.NewGuid().ToString()
                    }
                }).Verifiable();

            // Act
            var result = deviceTwinMapper.CreateDeviceDetails(twin);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()].ToString(), result.ModelId);
            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.LocationCode).ToCamelCase()].ToString(), result.LocationCode);
            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.AssetId).ToCamelCase()].ToString(), result.AssetId);
            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.DeviceType).ToCamelCase()].ToString(), result.DeviceType);

            Assert.IsTrue(result.AlreadyLoggedInOnce);

            Assert.AreEqual(twin.Properties.Desired[nameof(DeviceDetails.AppEUI)].ToString(), result.AppEUI);
            Assert.AreEqual(twin.Properties.Desired[nameof(DeviceDetails.AppKey)].ToString(), result.AppKey);
            Assert.AreEqual(twin.Properties.Desired[nameof(DeviceDetails.SensorDecoder)].ToString(), result.SensorDecoder);

            Assert.AreEqual(1, result.Commands.Count);
            Assert.AreEqual("Frame", result.Commands[0].Frame);

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

            Assert.AreEqual(twin.Tags[nameof(DeviceDetails.LocationCode).ToCamelCase()].ToString(), result.LocationCode);
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
                DeviceType = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                AppEUI = Guid.NewGuid().ToString(),
                AppKey = Guid.NewGuid().ToString(),
                SensorDecoder = Guid.NewGuid().ToString()
            };

            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.LocationCode), item.LocationCode);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.AssetId), item.AssetId);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.DeviceType), item.DeviceType);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.ModelId), item.ModelId);

            // Update the twin properties
            twin.Properties.Desired[nameof(item.AppEUI)] = item.AppEUI;
            twin.Properties.Desired[nameof(item.AppKey)] = item.AppKey;
            twin.Properties.Desired[nameof(item.SensorDecoder)] = item.SensorDecoder;

            // Act
            deviceTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.AppEUI, twin.Properties.Desired[nameof(DeviceDetails.AppEUI)].ToString());
            Assert.AreEqual(item.AppKey, twin.Properties.Desired[nameof(DeviceDetails.AppKey)].ToString());
            Assert.AreEqual(item.SensorDecoder, twin.Properties.Desired[nameof(DeviceDetails.SensorDecoder)].ToString());

            Assert.AreEqual(item.LocationCode, twin.Tags[nameof(DeviceDetails.LocationCode).ToCamelCase()].ToString());
            Assert.AreEqual(item.AssetId, twin.Tags[nameof(DeviceDetails.AssetId).ToCamelCase()].ToString());
            Assert.AreEqual(item.DeviceType, twin.Tags[nameof(DeviceDetails.DeviceType).ToCamelCase()].ToString());
            Assert.AreEqual(item.ModelId, twin.Tags[nameof(DeviceDetails.ModelId).ToCamelCase()].ToString());

            this.mockRepository.VerifyAll();
        }
    }
}
