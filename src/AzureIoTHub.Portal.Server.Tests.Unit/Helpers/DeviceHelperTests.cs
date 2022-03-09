// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Helpers
{
    using AzureIoTHub.Portal.Server.Helpers;
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
            var deviceId = "sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6";
            var attestation =
                new SymmetricKeyAttestation("8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw==",
                                            "8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw==");
            var expected = "Jsm0lyGpjaVYVP2g3FnmnmG9dI/9qU24wNoykUmermc=";

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

        [Test()]
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
            var expectedTagName = "myTagName";
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

            var propertyName = "propName";
            twin.Properties.Desired[propertyName] = "bbb";

            // Act
            var result = DeviceHelper.RetrieveDesiredPropertyValue(twin, propertyName);

            // Assert
            Assert.AreEqual("bbb", result);
        }

        [Test]
        public void WhenPropertyNotExistRetrieveDesiredPropertyValueShouldReturnNull()
        {
            // Arrange
            var twin = new Twin();

            var propertyName = "propName";

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

            var propertyName = "propName";
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

            var propertyName = "propName";

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
            var deviceId = "aaa";
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
            var deviceId = "aaa";
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
            var deviceId = "aaa";
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

            string deviceId = null;

            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin, deviceId);

            // Assert
            Assert.AreEqual(runtimeStatus, result);
        }

        [Test]
        public void WhenSystemModulesNotExistRetrieveRuntimeResponseShouldReturnEmptyString()
        {
            // Arrange
            var twin = new Twin();

            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin, "aaa");

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
            var result = DeviceHelper.RetrieveRuntimeResponse(twin, "aaa");

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
            var result = DeviceHelper.RetrieveRuntimeResponse(twin, "aaa");

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
                        status = "running"
                    },
                    LoRaBasicsStationModule = new
                    {
                        runtimeStatus = "running",
                        version = "1.0"
                    }
                }
            });

            var moduleCount = 2;

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
            var moduleCount = 0;
            var twin = new Twin();

            // Act
            var result = DeviceHelper.RetrieveModuleList(twin);

            // Assert
            Assert.AreEqual(moduleCount, result.Count);
        }
    }
}
