// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Helpers
{
    using System;
    using AzureIoTHub.Portal.Server.Helpers;
    using Microsoft.Azure.Devices.Shared;
    using NUnit.Framework;

    public class HelpersTest
    {
        [Test]
        public void RetrieveTagValueRealTwinReturnsExpectedValue()
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
            Assert.IsTrue(result.Contains(TAG_VALUE, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void RetrieveTagValueEmptyTwinReturnsNull()
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
        public void RetrieveProperyValueRealTwinReturnsExpectedValue()
        {
            // Arrange
            var twin = new Twin();
            const string PROPERTY_VALUE = "property_value";
            const string PROPERTY_KEY = "property_key";
            twin.Properties.Desired[PROPERTY_KEY] = PROPERTY_VALUE;

            // Act
            var result = DeviceHelper.RetrieveDesiredPropertyValue(twin, PROPERTY_KEY);

            // Assert
            Assert.IsTrue(result.Contains(PROPERTY_VALUE, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void RetrievePropertyValueEmptyTwinReturnsNull()
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
