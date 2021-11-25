using Microsoft.Azure.Devices.Provisioning.Service;
using Microsoft.Azure.Devices.Shared;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests
{
    class HelpersTest
    {
        [Test]
        public void RetrieveTagValue_RealTwin_ReturnsExpectedValue()
        {
            // Arrange
            var helpers = new Helpers.Helpers();
            var twin = new Twin();
            const string TAG_VALUE = "tag_value";
            const string TAG_KEY = "tag_key";
            twin.Tags[TAG_KEY] = TAG_VALUE;
            // moqTwin.Properties.Desired["AppEUI"] = device.AppEUI;
            
            // Act
            var method = typeof(Helpers.Helpers).GetMethod("RetrieveTagValue", BindingFlags.Static | BindingFlags.Public);
            var result = (string)method.Invoke(helpers, new object[] { twin, TAG_KEY });

            // Assert
            result.Contains($"{TAG_VALUE}");
        }

        [Test]
        public void RetrieveTagValue_EmptyTwin_ReturnsUndefined()
        {
            // Arrange
            var helpers = new Helpers.Helpers();
            var twin = new Twin();
            const string TAG_VALUE = "tag_value";
            const string TAG_KEY = "tag_key";

            // Act
            var method = typeof(Helpers.Helpers).GetMethod("RetrieveTagValue", BindingFlags.Static | BindingFlags.Public);
            var result = (string)method.Invoke(helpers, new object[] { twin, TAG_KEY });

            // Assert
            result.Contains($"undefined_{TAG_KEY}");
        }

        [Test]
        public void RetrieveProperyValue_RealTwin_ReturnsExpectedValue()
        {
            // Arrange
            var helpers = new Helpers.Helpers();
            var twin = new Twin();
            const string PROPERTY_VALUE = "property_value";
            const string PROPERTY_KEY = "property_key";
            twin.Properties.Desired[PROPERTY_KEY] = PROPERTY_VALUE;

            // Act
            var method = typeof(Helpers.Helpers).GetMethod("RetrievePropertyValue", BindingFlags.Static | BindingFlags.Public);
            var result = (string)method.Invoke(helpers, new object[] { twin, PROPERTY_KEY });

            // Assert
            result.Contains($"{PROPERTY_VALUE}");
        }

        [Test]
        public void RetrievePropertyValue_EmptyTwin_ReturnsUndefined()
        {
            // Arrange
            var helpers = new Helpers.Helpers();
            var twin = new Twin();
            const string PROPERTY_VALUE = "tag_value";
            const string PROPERTY_KEY = "tag_key";

            // Act
            var method = typeof(Helpers.Helpers).GetMethod("RetrievePropertyValue", BindingFlags.Static | BindingFlags.Public);
            var result = (string)method.Invoke(helpers, new object[] { twin, PROPERTY_KEY });

            // Assert
            result.Contains($"undefined_{PROPERTY_KEY}");
        }
    }
}
