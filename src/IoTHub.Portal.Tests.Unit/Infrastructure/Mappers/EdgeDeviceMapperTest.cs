// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
{
    [TestFixture]
    public class EdgeDeviceMapperTest
    {
        private MockRepository mockRepository;

        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceModelImageManager = this.mockRepository.Create<IDeviceModelImageManager>();
        }

        private EdgeDeviceMapper CreateMapper()
        {
            return new EdgeDeviceMapper(this.mockDeviceModelImageManager.Object);
        }

        [Test]
        public void CreateEdgeDeviceListItemShouldReturnValue()
        {
            // Arrange
            var edgeDeviceMapper = CreateMapper();

            var deviceTwin = new Twin(Guid.NewGuid().ToString());
            var modelId = Guid.NewGuid().ToString();

            deviceTwin.Properties.Reported["clients"] = new object[12];
            deviceTwin.Tags["modelId"] = modelId;

            _ = this.mockDeviceModelImageManager
                .Setup(x => x.GetDeviceModelImageAsync(It.Is<string>(c => c.Equals(modelId, StringComparison.Ordinal))).Result)
                .Returns(DeviceModelImageOptions.DefaultImage);

            // Act
            var result = edgeDeviceMapper.CreateEdgeDeviceListItem(deviceTwin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(12, result.NbDevices);
        }

        [Test]
        public void CreateEdgeDeviceShouldReturnValue()
        {
            // Arrange
            var edgeDeviceMapper = CreateMapper();

            var deviceId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var deviceTwin = new Twin(deviceId);
            deviceTwin.Tags["modelId"] = modelId;
            deviceTwin.Tags["location"] = "test";

            var tags = new List<string>(){"location"};

            var deviceTwinWithModules = new Twin(deviceId);
            var reportedProperties = new Dictionary<string, object>()
            {
                {
                    "systemModules", new Dictionary<string, object>()
                    {
                        {
                            "edgeAgent", new Dictionary<string, object>()
                            {
                                {
                                    "runtimeStatus", "running"
                                }
                            }
                        }
                    }
                }
            };

            deviceTwinWithModules.Properties.Desired["modules"] = new object[2];
            deviceTwinWithModules.Properties.Reported = new TwinCollection(JsonSerializer.Serialize(reportedProperties));

            var lastDeployment = new ConfigItem();

            _ = this.mockDeviceModelImageManager
                .Setup(x => x.GetDeviceModelImageAsync(It.Is<string>(c => c.Equals(modelId, StringComparison.Ordinal))).Result)
                .Returns(DeviceModelImageOptions.DefaultImage);

            // Act
            var result = edgeDeviceMapper.CreateEdgeDevice(deviceTwin, deviceTwinWithModules, 5, lastDeployment, tags);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(deviceTwin.DeviceId, result.DeviceId);
            Assert.AreEqual(5, result.NbDevices);
            Assert.AreEqual("running", result.RuntimeResponse);
            Assert.AreEqual(1, result.Tags.Count);
        }
    }
}
