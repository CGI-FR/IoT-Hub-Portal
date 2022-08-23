// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Mappers
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Mappers;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeDeviceMapperTest
    {

        [Test]
        public void CreateEdgeDeviceListItemShouldReturnValue()
        {
            // Arrange
            var edgeDeviceMapper = new EdgeDeviceMapper();

            var deviceTwin = new Twin(Guid.NewGuid().ToString());
            deviceTwin.Tags["type"] = "lora";
            deviceTwin.Properties.Reported["clients"] = new object[12];

            // Act
            var result = edgeDeviceMapper.CreateEdgeDeviceListItem(deviceTwin);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("lora", result.Type);
            Assert.AreEqual(12, result.NbDevices);
        }

        [Test]
        public void CreateEdgeDeviceShouldReturnValue()
        {
            // Arrange
            var edgeDeviceMapper = new EdgeDeviceMapper();

            var deviceId = Guid.NewGuid().ToString();

            var deviceTwin = new Twin(deviceId);
            deviceTwin.Tags["type"] = "fake";
            deviceTwin.Tags["env"] = "fake";

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
            deviceTwinWithModules.Properties.Reported = new TwinCollection(JsonConvert.SerializeObject(reportedProperties));

            var lastDeployment = new ConfigItem();

            // Act
            var result = edgeDeviceMapper.CreateEdgeDevice(deviceTwin, deviceTwinWithModules, 5, lastDeployment);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(deviceTwin.DeviceId, result.DeviceId);
            Assert.AreEqual(5, result.NbDevices);
            Assert.AreEqual("running", result.RuntimeResponse);
            Assert.AreEqual("fake", result.Type);
        }
    }
}
