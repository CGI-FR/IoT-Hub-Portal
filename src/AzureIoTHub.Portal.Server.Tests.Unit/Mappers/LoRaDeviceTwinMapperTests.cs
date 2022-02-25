using AzureIoTHub.Portal.Server.Helpers;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
using Microsoft.Azure.Devices.Shared;
using Moq;
using NUnit.Framework;
using System;

namespace AzureIoTHub.Portal.Server.Tests.Mappers
{
    [TestFixture]
    public class LoRaDeviceTwinMapperTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
        }

        private LoRaDeviceTwinMapper CreateLoRaDeviceTwinMapper()
        {
            return new LoRaDeviceTwinMapper(
                this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void CreateDeviceDetails_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var loRaDeviceTwinMapper = this.CreateLoRaDeviceTwinMapper();
            Twin twin = new Twin(Guid.NewGuid().ToString());
            string modelId = Guid.NewGuid().ToString();

            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.ModelId), modelId);
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.DeviceName), Guid.NewGuid().ToString());
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.AssetId), Guid.NewGuid().ToString());
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.LocationCode), Guid.NewGuid().ToString());

            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEUI)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)] = Guid.NewGuid().ToString();

            var expectedModelImageUri = $"https://fake.local/{modelId}";

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == modelId)))
                .Returns(expectedModelImageUri);

            // Act
            var result = loRaDeviceTwinMapper.CreateDeviceDetails(twin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(twin.DeviceId, result.DeviceID);
            Assert.AreEqual(modelId, result.ModelId);
            Assert.AreEqual(DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)), result.DeviceName);
            Assert.AreEqual(DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.LocationCode)), result.LocationCode);
            Assert.AreEqual(DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.AssetId)), result.AssetId);

            Assert.AreEqual(expectedModelImageUri, result.ImageUrl);

            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEUI)].ToString(), result.AppEUI);
            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)].ToString(), result.AppKey);
            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)].ToString(), result.SensorDecoder);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void CreateDeviceListItem_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var loRaDeviceTwinMapper = this.CreateLoRaDeviceTwinMapper();
            Twin twin = new Twin(Guid.NewGuid().ToString());
            string modelId = Guid.NewGuid().ToString();

            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.ModelId), modelId);
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.DeviceName), Guid.NewGuid().ToString());

            var expectedModelImageUri = $"https://fake.local/{modelId}";

            this.mockDeviceModelImageManager.Setup(c => c.ComputeImageUri(It.Is<string>(x => x == modelId)))
                .Returns(expectedModelImageUri);

            // Act
            var result = loRaDeviceTwinMapper.CreateDeviceListItem(twin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(twin.DeviceId, result.DeviceID);
            Assert.AreEqual(DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)), result.DeviceName);

            Assert.AreEqual(expectedModelImageUri, result.ImageUrl);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTwin_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var loRaDeviceTwinMapper = this.CreateLoRaDeviceTwinMapper();
            Twin twin = new Twin(Guid.NewGuid().ToString());

            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.ModelId), Guid.NewGuid().ToString());
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.DeviceName), Guid.NewGuid().ToString());
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.AssetId), Guid.NewGuid().ToString());
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.LocationCode), Guid.NewGuid().ToString());

            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEUI)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)] = Guid.NewGuid().ToString();

            LoRaDeviceDetails item = new LoRaDeviceDetails
            {
                DeviceID = twin.DeviceId,
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                AssetId = Guid.NewGuid().ToString(),
                LocationCode = Guid.NewGuid().ToString(),
                AppEUI = Guid.NewGuid().ToString(),
                AppKey = Guid.NewGuid().ToString(),
                SensorDecoder = Guid.NewGuid().ToString(),
            };

            // Act
            loRaDeviceTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.DeviceID, twin.DeviceId);
            Assert.AreEqual(item.ModelId, DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.ModelId)));
            Assert.AreEqual(item.DeviceName, DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)));
            Assert.AreEqual(item.LocationCode, DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.LocationCode)));
            Assert.AreEqual(item.AssetId, DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.AssetId)));

            Assert.AreEqual(item.AppEUI, twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEUI)].ToString());
            Assert.AreEqual(item.AppKey, twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)].ToString());
            Assert.AreEqual(item.SensorDecoder, twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)].ToString());

            this.mockRepository.VerifyAll();
        }
    }
}
