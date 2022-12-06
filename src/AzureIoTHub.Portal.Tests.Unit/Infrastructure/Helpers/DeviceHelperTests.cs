// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Helpers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Application.Helpers;
    using AzureIoTHub.Portal.Crosscutting.Extensions;
    using AzureIoTHub.Portal.Domain.Entities;
    using FluentAssertions;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceHelperTests
    {
        [Test]
        public void RetrieveSymmetricKeyShouldReturnDerivedKey()
        {
            // Arrange
            const string deviceId = "sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6";
            var attestation =
                new SymmetricKeyAttestation("8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw==",
                                            "8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw==");
            const string expected = "Jsm0lyGpjaVYVP2g3FnmnmG9dI/9qU24wNoykUmermc=";

            // Act
            var result = DeviceHelper.RetrieveSymmetricKey(deviceId, attestation);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase("myTagName")]
        [TestCase("MyTagName")]
        public void RetrieveTagValueShouldFindCamelCasedTag(string searchedTagName)
        {
            // Arrange
            var item = new Twin();
            item.Tags["myTagName"] = "bbb";

            // Act
            var result = DeviceHelper.RetrieveTagValue(
                item,
                searchedTagName);

            // Assert
            Assert.AreEqual("bbb", result);
        }

        [Test]
        public void WhenNotPresentRetrieveTagValueShouldReturnNull()
        {
            // Arrange
            var item = new Twin();
            item.Tags["myTagName"] = "bbb";

            // Act
            var result = DeviceHelper.RetrieveTagValue(
                item,
                "otherTag");

            // Assert
            Assert.IsNull(result);
        }

        [TestCase("myTagName")]
        [TestCase("MyTagName")]
        public void SetTagValueShouldRegisterTagWithCamelCase(string registeredTagName)
        {
            // Arrange
            var item = new Twin();
            const string expectedTagName = "myTagName";
            var value = string.Empty;

            // Act
            DeviceHelper.SetTagValue(item, registeredTagName, value);

            // Assert
            Assert.IsTrue(item.Tags.Contains(expectedTagName));
        }

        [Test]
        public void RetrieveDesiredPropertyValueShouldReturnDesiredProperty()
        {
            // Arrange
            var twin = new Twin();

            const string propertyName = "propName";
            twin.Properties.Desired[propertyName] = "bbb";

            // Act
            var result = DeviceHelper.RetrieveDesiredPropertyValue(twin, propertyName);

            // Assert
            Assert.AreEqual("bbb", result);
        }

        [Test]
        public void SetDesiredPropertyShouldReturnTwinWithNewValue()
        {
            // Arrange
            var item = new Twin();
            const string expectedTagName = "myTagName";
            var value = string.Empty;

            // Act
            DeviceHelper.SetDesiredProperty(item, expectedTagName, value);

            // Assert
            Assert.IsTrue(item.Properties.Desired.Contains(expectedTagName));
        }

        [Test]
        public void WhenPropertyNotExistRetrieveDesiredPropertyValueShouldReturnNull()
        {
            // Arrange
            var twin = new Twin();

            const string propertyName = "propName";

            // Act
            var result = DeviceHelper.RetrieveDesiredPropertyValue(twin, propertyName);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void RetrieveReportedPropertyValueShouldReturnReportedProperty()
        {
            // Arrange
            var twin = new Twin();

            const string propertyName = "propName";
            twin.Properties.Reported[propertyName] = "bbb";

            // Act
            var result = DeviceHelper.RetrieveReportedPropertyValue(twin, propertyName);

            // Assert
            Assert.AreEqual("bbb", result);
        }

        [Test]
        public void WhenPropertyNotExistRetrieveReportedPropertyValueShouldReturnNull()
        {
            // Arrange
            var twin = new Twin();

            const string propertyName = "propName";

            // Act
            var result = DeviceHelper.RetrieveReportedPropertyValue(twin, propertyName);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void RetrieveConnectedDeviceCountShouldReturnClientArrayCountFromReportedProperty()
        {
            // Arrange
            var twin = new Twin();
            twin.Properties.Reported["clients"] = new object[12];

            // Act
            var result = DeviceHelper.RetrieveConnectedDeviceCount(twin);

            // Assert
            Assert.AreEqual(12, result);
        }

        [Test]
        public void WhenPropertyNotExistRetrieveConnectedDeviceCountShouldReturn0()
        {
            // Arrange
            var twin = new Twin();

            // Act
            var result = DeviceHelper.RetrieveConnectedDeviceCount(twin);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void RetrieveNbModuleCountShouldReturnModuleArrayCountFromDesiredProperty()
        {
            // Arrange
            const string deviceId = "aaa";
            var twin = new Twin(deviceId);
            twin.Properties.Desired["modules"] = new object[12];

            // Act
            var result = DeviceHelper.RetrieveNbModuleCount(twin, deviceId);

            // Assert
            Assert.AreEqual(12, result);
        }

        [Test]
        public void WhenTwinIsDifferentDeviceIdRetrieveNbModuleCountShouldReturn0()
        {
            // Arrange
            const string deviceId = "aaa";
            var twin = new Twin();
            twin.Properties.Desired["modules"] = new object[12];

            // Act
            var result = DeviceHelper.RetrieveNbModuleCount(twin, deviceId);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void WhenPropertyNotExistRetrieveNbModuleCountShouldReturn0()
        {
            // Arrange
            const string deviceId = "aaa";
            var twin = new Twin(deviceId);

            // Act
            var result = DeviceHelper.RetrieveNbModuleCount(twin, deviceId);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestCase("unknown")]
        [TestCase("running")]
        public void RetrieveRuntimeResponseShouldReturnEdgeAgentSystemModuleRuntimeStatus(string runtimeStatus)
        {
            // Arrange
            var twin = new Twin();
            twin.Properties.Reported["systemModules"] = new
            {
                edgeAgent = new
                {
                    runtimeStatus
                }
            };

            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin);

            // Assert
            Assert.AreEqual(runtimeStatus, result);
        }

        [Test]
        public void WhenSystemModulesNotExistRetrieveRuntimeResponseShouldReturnEmptyString()
        {
            // Arrange
            var twin = new Twin();

            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void WhenEdgeAgentNotExistRetrieveRuntimeResponseShouldReturnEmptyString()
        {
            // Arrange
            var twin = new Twin();
            twin.Properties.Reported["systemModules"] = new
            {
            };
            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void WhenRuntimeStatusNotExistRetrieveRuntimeResponseShouldReturnEmptyString()
        {
            // Arrange
            var twin = new Twin();
            twin.Properties.Reported["systemModules"] = new
            {
                edgeAgent = new
                {
                }
            };

            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void RetrieveModuleListShouldReturnModuleList()
        {
            // Arrange
            var modulesJson = JsonConvert.SerializeObject(new
            {
                modules = new
                {
                    LoRaWanNetworkSrvModule = new
                    {
                        version = "1.0",
                        status = "running",
                        settings = new
                        {
                            image = "image"
                        }
                    },
                    LoRaBasicsStationModule = new
                    {
                        runtimeStatus = "running",
                        version = "1.0",
                        settings = new
                        {
                            image = "image"
                        }
                    }
                }
            });

            const int moduleCount = 2;

            var twin = new Twin(new TwinProperties
            {
                Reported = new TwinCollection(modulesJson)
            });

            // Act
            var result = DeviceHelper.RetrieveModuleList(twin);

            // Assert
            Assert.AreEqual(moduleCount, result.Count);
        }

        [Test]
        public void WhenPropertyNotExistRetrieveModuleListShouldReturnEmptyList()
        {
            // Arrange
            const int moduleCount = 0;
            var twin = new Twin();

            // Act
            var result = DeviceHelper.RetrieveModuleList(twin);

            // Assert
            Assert.AreEqual(moduleCount, result.Count);
        }

        [Test]
        public void PropertiesWithDotNotationToTwinCollectionMustParseClassicPropertiesAndDotNotationProperties()
        {
            // Arrange
            var input = new Dictionary<string, object>()
            {
                ["firmware.url"] = "fake.url.com",
                ["firmware.version"] = 1.5,
                ["enabled"] = true,
            };

            // Act
            var result = DeviceHelper.PropertiesWithDotNotationToTwinCollection(input);

            // Assert
            string firmwareUrl = Convert.ToString(result["firmware"]["url"].Value);
            string firmwareVersion = Convert.ToString(result["firmware"]["version"].Value);
            string enabled = Convert.ToString(result["enabled"].Value);

            _ = firmwareUrl.Should().Be("fake.url.com");
            _ = firmwareVersion.Should().Be("1.5");
            _ = enabled.Should().Be("True");
        }

        [Test]
        public void RetrieveTagValueRealTwinReturnsExpectedValue()
        {
            // Arrange
            var twin = new Twin();
            const string TAG_VALUE = "tag_value";
            const string TAG_KEY = "tag_key";
            twin.Tags[TAG_KEY] = TAG_VALUE;

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

        [Test]
        public void RetrieveClientThumbprintShouldReturnClientThumbprintValue()
        {
            // Arrange
            var expectedThumbprint = Guid.NewGuid().ToString();
            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };
            twin.Properties.Desired[nameof(Concentrator.ClientThumbprint).ToCamelCase()] = new List<string>() { expectedThumbprint };


            // Act
            var result = DeviceHelper.RetrieveClientThumbprintValue(twin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedThumbprint, result);
        }

        [Test]
        public void RetrieveClientThumbprintValueNotExistShouldReturnNull()
        {
            // Arrange
            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            // Act
            var result = DeviceHelper.RetrieveClientThumbprintValue(twin);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void RetrieveClientThumbprintValueEmptyArrayShouldReturnNull()
        {
            // Arrange
            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };
            twin.Properties.Desired[nameof(Concentrator.ClientThumbprint).ToCamelCase()] = new List<string>();

            // Act
            var result = DeviceHelper.RetrieveClientThumbprintValue(twin);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void CreateDeviceDetailsClientThumbprintBadFormatShouldReturnNull()
        {
            // Arrange
            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };
            twin.Properties.Desired[nameof(Concentrator.ClientThumbprint).ToCamelCase()] = Guid.NewGuid().ToString();

            // Act
            var result = DeviceHelper.RetrieveClientThumbprintValue(twin);

            // Assert
            Assert.IsNull(result);
        }
    }
}
