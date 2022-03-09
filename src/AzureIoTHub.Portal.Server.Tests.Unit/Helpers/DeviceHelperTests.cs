using AzureIoTHub.Portal.Server.Helpers;
using Microsoft.Azure.Devices.Provisioning.Service;
using Microsoft.Azure.Devices.Shared;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Helpers
{
    [TestFixture]
    public class DeviceHelperTests
    {
        [Test]
        public void RetrieveSymmetricKey_Should_Return_Derived_Key()
        {
            // Arrange
            string deviceId = "sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6";
            SymmetricKeyAttestation attestation =
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
        public void RetrieveTagValue_Should_Find_CamelCased_Tag(string searchedTagName)
        {
            // Arrange
            Twin item = new Twin();
            item.Tags["myTagName"] = "bbb";

            // Act
            var result = DeviceHelper.RetrieveTagValue(
                item,
                searchedTagName);

            // Assert
            Assert.AreEqual("bbb", result);
        }

        [Test()]
        public void When_Not_Present_RetrieveTagValue_Should_Return_Null()
        {
            // Arrange
            Twin item = new Twin();
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
        public void SetTagValue_ShouldRegister_Tag_With_CamelCase(string registeredTagName)
        {
            // Arrange
            Twin item = new Twin();
            string expectedTagName = "myTagName";
            string value = string.Empty;

            // Act
            DeviceHelper.SetTagValue(item, registeredTagName, value);

            // Assert
            Assert.IsTrue(item.Tags.Contains(expectedTagName));
        }

        [Test]
        public void RetrieveDesiredPropertyValue_Should_Return_Desired_Property()
        {
            // Arrange
            Twin twin = new Twin();

            string propertyName = "propName";
            twin.Properties.Desired[propertyName] = "bbb";

            // Act
            var result = DeviceHelper.RetrieveDesiredPropertyValue(twin, propertyName);

            // Assert
            Assert.AreEqual("bbb", result);
        }

        [Test]
        public void SetDesiredProperty_Should_Return_twin_with_new_value()
        {
            // Arrange
            Twin item = new Twin();
            string expectedTagName = "myTagName";
            string value = string.Empty;

            // Act
            DeviceHelper.SetDesiredProperty(item, expectedTagName, value);

            // Assert
            Assert.IsTrue(item.Properties.Desired.Contains(expectedTagName));
        }

        [Test]
        public void When_Property_Not_Exist_RetrieveDesiredPropertyValue_Should_Return_Null()
        {
            // Arrange
            Twin twin = new Twin();

            string propertyName = "propName";

            // Act
            var result = DeviceHelper.RetrieveDesiredPropertyValue(twin, propertyName);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void RetrieveReportedPropertyValue_Should_Return_Reported_Property()
        {
            // Arrange
            Twin twin = new Twin();

            string propertyName = "propName";
            twin.Properties.Reported[propertyName] = "bbb";

            // Act
            var result = DeviceHelper.RetrieveReportedPropertyValue(twin, propertyName);

            // Assert
            Assert.AreEqual("bbb", result);
        }

        [Test]
        public void When_Property_Not_Exist_RetrieveReportedPropertyValue_Should_Return_Null()
        {
            // Arrange
            Twin twin = new Twin();

            string propertyName = "propName";

            // Act
            var result = DeviceHelper.RetrieveReportedPropertyValue(twin, propertyName);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void RetrieveConnectedDeviceCount_Should_Return_Client_Array_Count_From_Reported_Property()
        {
            // Arrange
            Twin twin = new Twin();
            twin.Properties.Reported["clients"] = new object[12];

            // Act
            var result = DeviceHelper.RetrieveConnectedDeviceCount(twin);

            // Assert
            Assert.AreEqual(12, result);
        }

        [Test]
        public void When_Property_Not_Exist_RetrieveConnectedDeviceCount_Should_Return_0()
        {
            // Arrange
            Twin twin = new Twin();

            // Act
            var result = DeviceHelper.RetrieveConnectedDeviceCount(twin);

            // Assert
            Assert.AreEqual(0, result);
        }


        [Test]
        public void RetrieveNbModuleCount_Should_Return_Module_Array_Count_From_Desired_Property()
        {
            // Arrange
            var deviceId = "aaa";
            Twin twin = new Twin(deviceId);
            twin.Properties.Desired["modules"] = new object[12];

            // Act
            var result = DeviceHelper.RetrieveNbModuleCount(twin, deviceId);

            // Assert
            Assert.AreEqual(12, result);
        }

        [Test]
        public void When_Twin_Is_Different_Device_Id_RetrieveNbModuleCount_Should_Return_0()
        {
            // Arrange
            var deviceId = "aaa";
            Twin twin = new Twin();
            twin.Properties.Desired["modules"] = new object[12];

            // Act
            var result = DeviceHelper.RetrieveNbModuleCount(twin, deviceId);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void When_Property_Not_Exist_RetrieveNbModuleCount_Should_Return_0()
        {
            // Arrange
            var deviceId = "aaa";
            Twin twin = new Twin(deviceId);

            // Act
            var result = DeviceHelper.RetrieveNbModuleCount(twin, deviceId);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestCase("unknown")]
        [TestCase("running")]
        public void RetrieveRuntimeResponse_Should_Return_Edge_Agent_System_Module_Runtime_Status(string runtimeStatus)
        {
            // Arrange
            Twin twin = new Twin();
            twin.Properties.Reported["systemModules"] = new
            {
                edgeAgent = new
                {
                    runtimeStatus = runtimeStatus
                }
            };

            string deviceId = null;

            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin, deviceId);

            // Assert
            Assert.AreEqual(runtimeStatus, result);
        }

        [Test]
        public void When_SystemModules_Not_Exist_RetrieveRuntimeResponse_Should_Return_Empty_String()
        {
            // Arrange
            Twin twin = new Twin();

            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin, "aaa");

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void When_EdgeAgent_Not_Exist_RetrieveRuntimeResponse_Should_Return_Empty_String()
        {
            // Arrange
            Twin twin = new Twin();
            twin.Properties.Reported["systemModules"] = new
            {
            };
            // Act
            var result = DeviceHelper.RetrieveRuntimeResponse(twin, "aaa");

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void When_RuntimeStatus_Not_Exist_RetrieveRuntimeResponse_Should_Return_Empty_String()
        {
            // Arrange
            Twin twin = new Twin();
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
        public void RetrieveModuleList_Should_Return_Module_List()
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
        public void When_Property_Not_Exist_RetrieveModuleList_Should_Return_Empty_List()
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
