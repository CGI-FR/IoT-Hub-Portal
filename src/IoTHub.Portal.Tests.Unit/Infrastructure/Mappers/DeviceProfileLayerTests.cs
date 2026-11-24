// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
{
    using DeviceEntity = Portal.Domain.Entities.Device;

    [TestFixture]
    public class DeviceProfileLayerTests : BackendUnitTest
    {
        public override void Setup()
        {
            base.Setup();
            Services = ServiceCollection.BuildServiceProvider();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("None", null)]
        [TestCase("layer-1", "layer-1")]
        public void MapTwin_LayerTag_NormalizesBothDeviceTypes(string layerId, string expected)
        {
            var twin = new Twin("device");
            twin.Tags["deviceName"] = "Device";
            twin.Tags["modelId"] = "model";
            if (layerId != null) twin.Tags["layerId"] = layerId;

            var device = Mapper.Map<DeviceEntity>(twin);
            var lorawanDevice = Mapper.Map<LorawanDevice>(twin);

            Assert.AreEqual(expected, device.LayerId);
            Assert.AreEqual(expected, lorawanDevice.LayerId);
            Assert.IsFalse(device.Tags.Any(tag => tag.Name == "layerId"));
            Assert.IsFalse(lorawanDevice.Tags.Any(tag => tag.Name == "layerId"));
        }
    }
}
