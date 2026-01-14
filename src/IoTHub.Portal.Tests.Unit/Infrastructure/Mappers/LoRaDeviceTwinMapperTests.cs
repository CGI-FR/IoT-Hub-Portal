// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
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
            return new LoRaDeviceTwinMapper(this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void CreateDeviceDetailsStateUnderTestExpectedBehavior()
        {
            // Arrange
            var loRaDeviceTwinMapper = CreateLoRaDeviceTwinMapper();
            var twin = new Twin(Guid.NewGuid().ToString());
            var modelId = Guid.NewGuid().ToString();

            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.ModelId), modelId);
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.DeviceName), Guid.NewGuid().ToString());

            twin.Tags["assetId"] = Guid.NewGuid().ToString();
            twin.Tags["locationCode"] = Guid.NewGuid().ToString();
            var tagsNames = new List<string>() { "assetId", "locationCode" };

            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEui)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)] = Guid.NewGuid().ToString();

            const string modelImage = DeviceModelImageOptions.DefaultImage;

            _ = this.mockDeviceModelImageManager.Setup(c => c.GetDeviceModelImageAsync(It.Is<string>(x => x == modelId)).Result)
                .Returns(modelImage);

            // Act
            var result = loRaDeviceTwinMapper.CreateDeviceDetails(twin, tagsNames);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(twin.DeviceId, result.DeviceId);
            Assert.AreEqual(modelId, result.ModelId);
            Assert.AreEqual(DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)), result.DeviceName);

            foreach (var tagName in tagsNames)
            {
                Assert.AreEqual(DeviceHelper.RetrieveTagValue(twin, tagName), result.Tags[tagName]);
            }

            Assert.AreEqual(modelImage, result.Image);

            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEui)].ToString(), result.AppEui);
            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)].ToString(), result.AppKey);
            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)].ToString(), result.SensorDecoder);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void CreateDeviceDetailsNullTagListExpectedBehavior()
        {
            // Arrange
            var loRaDeviceTwinMapper = CreateLoRaDeviceTwinMapper();
            var twin = new Twin(Guid.NewGuid().ToString());
            var modelId = Guid.NewGuid().ToString();

            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.ModelId), modelId);
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.DeviceName), Guid.NewGuid().ToString());

            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEui)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)] = Guid.NewGuid().ToString();

            const string modelImage = DeviceModelImageOptions.DefaultImage;

            _ = this.mockDeviceModelImageManager.Setup(c => c.GetDeviceModelImageAsync(It.Is<string>(x => x == modelId)).Result)
                .Returns(modelImage);

            // Act
            var result = loRaDeviceTwinMapper.CreateDeviceDetails(twin, new List<string>());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(twin.DeviceId, result.DeviceId);
            Assert.AreEqual(modelId, result.ModelId);
            Assert.AreEqual(DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)), result.DeviceName);

            Assert.IsEmpty(result.Tags);

            Assert.AreEqual(modelImage, result.Image);

            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEui)].ToString(), result.AppEui);
            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)].ToString(), result.AppKey);
            Assert.AreEqual(twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)].ToString(), result.SensorDecoder);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void CreateDeviceListItemStateUnderTestExpectedBehavior()
        {
            // Arrange
            var loRaDeviceTwinMapper = CreateLoRaDeviceTwinMapper();
            var twin = new Twin(Guid.NewGuid().ToString());
            var modelId = Guid.NewGuid().ToString();

            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.ModelId), modelId);
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.DeviceName), Guid.NewGuid().ToString());

            const string modelImage = DeviceModelImageOptions.DefaultImage;

            _ = this.mockDeviceModelImageManager.Setup(c => c.GetDeviceModelImageAsync(It.Is<string>(x => x == modelId)).Result)
                .Returns(modelImage);

            // Act
            var result = loRaDeviceTwinMapper.CreateDeviceListItem(twin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(twin.DeviceId, result.DeviceId);
            Assert.AreEqual(DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)), result.DeviceName);

            Assert.AreEqual(modelImage, result.Image);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTwinStateUnderTestExpectedBehavior()
        {
            // Arrange
            var loRaDeviceTwinMapper = CreateLoRaDeviceTwinMapper();
            var twin = new Twin(Guid.NewGuid().ToString());

            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.ModelId), Guid.NewGuid().ToString());
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.DeviceName), Guid.NewGuid().ToString());

            twin.Tags["assetId"] = Guid.NewGuid().ToString();
            twin.Tags["locationCode"] = Guid.NewGuid().ToString();
            var tagsNames = new List<string>() { "assetId", "locationCode" };

            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEui)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)] = Guid.NewGuid().ToString();

            var item = new LoRaDeviceDetails
            {
                DeviceId = twin.DeviceId,
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                AppEui = Guid.NewGuid().ToString(),
                AppKey = Guid.NewGuid().ToString(),
                SensorDecoder = Guid.NewGuid().ToString(),
                UseOtaa = true,
            };

            item.Tags.Add("assetId", Guid.NewGuid().ToString());
            item.Tags.Add("locationCode", Guid.NewGuid().ToString());

            // Act
            loRaDeviceTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.DeviceId, twin.DeviceId);
            Assert.AreEqual(item.ModelId, DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.ModelId)));
            Assert.AreEqual(item.DeviceName, DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)));

            foreach (var tagName in tagsNames)
            {
                Assert.AreEqual(item.Tags[tagName], DeviceHelper.RetrieveTagValue(twin, tagName));
            }

            Assert.AreEqual(item.AppEui, twin.Properties.Desired[nameof(LoRaDeviceDetails.AppEui)].ToString());
            Assert.AreEqual(item.AppKey, twin.Properties.Desired[nameof(LoRaDeviceDetails.AppKey)].ToString());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTwinUseOtaaIsFalseExpectedBehavior()
        {
            // Arrange
            var loRaDeviceTwinMapper = CreateLoRaDeviceTwinMapper();
            var twin = new Twin(Guid.NewGuid().ToString());

            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.ModelId), Guid.NewGuid().ToString());
            DeviceHelper.SetTagValue(twin, nameof(LoRaDeviceDetails.DeviceName), Guid.NewGuid().ToString());

            twin.Tags["assetId"] = Guid.NewGuid().ToString();
            twin.Tags["locationCode"] = Guid.NewGuid().ToString();
            var tagsNames = new List<string>() { "assetId", "locationCode" };

            twin.Properties.Desired[nameof(LoRaDeviceDetails.NwkSKey)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.AppSKey)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.DevAddr)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.GatewayId)] = Guid.NewGuid().ToString();
            twin.Properties.Desired[nameof(LoRaDeviceDetails.SensorDecoder)] = Guid.NewGuid().ToString();

            var item = new LoRaDeviceDetails
            {
                DeviceId = twin.DeviceId,
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                AppSKey = Guid.NewGuid().ToString(),
                NwkSKey = Guid.NewGuid().ToString(),
                DevAddr = Guid.NewGuid().ToString(),
                GatewayId = Guid.NewGuid().ToString(),
                SensorDecoder = Guid.NewGuid().ToString(),
                UseOtaa = false,
            };

            item.Tags.Add("assetId", Guid.NewGuid().ToString());
            item.Tags.Add("locationCode", Guid.NewGuid().ToString());

            // Act
            loRaDeviceTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.DeviceId, twin.DeviceId);
            Assert.AreEqual(item.ModelId, DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.ModelId)));
            Assert.AreEqual(item.DeviceName, DeviceHelper.RetrieveTagValue(twin, nameof(LoRaDeviceDetails.DeviceName)));

            foreach (var tagName in tagsNames)
            {
                Assert.AreEqual(item.Tags[tagName], DeviceHelper.RetrieveTagValue(twin, tagName));
            }

            Assert.AreEqual(item.NwkSKey, twin.Properties.Desired[nameof(LoRaDeviceDetails.NwkSKey)].ToString());
            Assert.AreEqual(item.AppSKey, twin.Properties.Desired[nameof(LoRaDeviceDetails.AppSKey)].ToString());
            Assert.AreEqual(item.DevAddr, twin.Properties.Desired[nameof(LoRaDeviceDetails.DevAddr)].ToString());
            Assert.AreEqual(item.GatewayId, twin.Properties.Desired[nameof(LoRaDeviceDetails.GatewayId)].ToString());

            this.mockRepository.VerifyAll();
        }
    }
}
