using AzureIoTHub.Portal.Server.Helpers;
using Microsoft.Azure.Devices.Shared;
using NUnit.Framework;

namespace AzureIoTHub.Portal.Server.Tests.Helpers
{
    class HelpersTest
    {
        [Test]
        public void RetrieveTagValue_RealTwin_ReturnsExpectedValue()
        {
            // Arrange
            var twin = new Twin();
            const string TAG_VALUE = "tag_value";
            const string TAG_KEY = "tag_key";
            twin.Tags[TAG_KEY] = TAG_VALUE;
            // moqTwin.Properties.Desired["AppEUI"] = device.AppEUI;

            // Act
            var result = DeviceHelper.RetrieveTagValue(twin, TAG_KEY);

            // Assert
            result.Contains($"{TAG_VALUE}");
        }

        [Test]
        public void RetrieveTagValue_EmptyTwin_ReturnsNull()
        {
            // Arrange
            var twin = new Twin();
            const string TAG_KEY = "tag_key";

            // Act
            var result = DeviceHelper.RetrieveTagValue(twin, TAG_KEY);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void RetrieveProperyValue_RealTwin_ReturnsExpectedValue()
        {
            // Arrange
            var twin = new Twin();
            const string PROPERTY_VALUE = "property_value";
            const string PROPERTY_KEY = "property_key";
            twin.Properties.Desired[PROPERTY_KEY] = PROPERTY_VALUE;

            // Act
            var result = DeviceHelper.RetrieveDesiredPropertyValue(twin, PROPERTY_KEY);

            // Assert
            result.Contains($"{PROPERTY_VALUE}");
        }

        [Test]
        public void RetrievePropertyValue_EmptyTwin_ReturnsNull()
        {
            // Arrange
            var twin = new Twin();
            const string PROPERTY_KEY = "tag_key";

            // Act
            var result = DeviceHelper.RetrieveDesiredPropertyValue(twin, PROPERTY_KEY);

            // Assert
            Assert.IsNull(result);
        }
    }
}
