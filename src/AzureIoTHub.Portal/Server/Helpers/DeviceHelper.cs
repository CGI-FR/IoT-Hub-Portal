// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using AzureIoTHub.Portal.Server.Extensions;
    using AzureIoTHub.Portal.Shared.Models;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;

    public static class DeviceHelper
    {
        /// <summary>
        /// This function genefates the symmetricKey of a device
        /// from its Id.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <returns>string.</returns>
        public static string RetrieveSymmetricKey(string deviceId, AttestationMechanism attestationMechanism)
        {
            // then we get the symmetricKey
            SymmetricKeyAttestation symmetricKey = attestationMechanism.GetAttestation() as SymmetricKeyAttestation;
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(symmetricKey.PrimaryKey));

            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId)));
        }

        /// <summary>
        /// Checks if the specific property exists within the device twin,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="item">the device twin.</param>
        /// <param name="tagName">the tag property.</param>
        /// <returns>string.</returns>
        public static string RetrieveTagValue(Twin item, string tagName)
        {
            var camelCasedTagName = tagName.ToCamelCase();

            if (item.Tags.Contains(camelCasedTagName))
                return item.Tags[camelCasedTagName];

            return null;
        }

        /// <summary>
        /// Sets the twin tag value.
        /// </summary>
        /// <param name="item">The device Twin.</param>
        /// <param name="tagName">The tag name.</param>
        /// <param name="value">The tag value.</param>
        public static void SetTagValue(Twin item, string tagName, string value)
        {
            var camelCasedTagName = tagName.ToCamelCase();

            item.Tags[camelCasedTagName] = value;
        }

        /// <summary>
        /// Checks if the specific property exists within the device twin,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="item">Device twin.</param>
        /// <param name="propertyName">Property to retrieve.</param>
        /// <returns>Corresponding property value, or null if it doesn't exist.</returns>
        public static string RetrieveDesiredPropertyValue(Twin item, string propertyName)
        {
            if (item.Properties.Desired.Contains(propertyName))
                return item.Properties.Desired[propertyName];
            else
                return null;
        }

        public static string RetrieveReportedPropertyValue(Twin twin, string propertyName)
        {
            if (twin.Properties.Reported.Contains(propertyName))
                return twin.Properties.Reported[propertyName];
            else
                return null;
        }

        /// <summary>
        /// this function retreive and return the number of connected
        /// devices.
        /// </summary>
        /// <param name="twin">the twin of the device.</param>
        /// <returns>the number of connected device.</returns>
        public static int RetrieveConnectedDeviceCount(Twin twin)
        {
            if (twin.Properties.Reported.Contains("clients"))
            {
                return twin.Properties.Reported["clients"].Count;
            }

            return 0;
        }

        /// <summary>
        /// This function get and return the number of module deployed,
        /// in the reported properties of the twin.
        /// </summary>
        /// <param name="twin">the twin of the device we want.</param>
        /// <param name="deviceId">the device id we get.</param>
        /// <returns>int.</returns>
        public static int RetrieveNbModuleCount(Twin twin, string deviceId)
        {
            if (twin.Properties.Desired.Contains("modules") && twin.DeviceId == deviceId)
                return twin.Properties.Desired["modules"].Count;
            else
                return 0;
        }

        /// <summary>
        /// This function get and return the runtime status of the module
        /// edgeAgent as the runtime response of the device.
        /// </summary>
        /// <param name="twin">the twin of the device we want.</param>
        /// <param name="deviceId">the device id we get.</param>
        /// <returns>string.</returns>
        public static string RetrieveRuntimeResponse(Twin twin, string deviceId)
        {
            if (twin.Properties.Reported.Contains("systemModules") && twin.DeviceId == deviceId)
            {
                foreach (var element in twin.Properties.Reported["systemModules"])
                {
                    if (element.Key == "edgeAgent")
                    {
                        return element.Value["runtimeStatus"];
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// This function get and return a list of the modules.
        /// </summary>
        /// <param name="twin">the twin of the device we want.</param>
        /// <returns> List of GatewayModule.</returns>
        public static List<GatewayModule> RetrieveModuleList(Twin twin, int moduleCount)
        {
            var list = new List<GatewayModule>();

            if (twin.Properties.Reported.Contains("modules") && moduleCount > 0)
            {
                foreach (var element in twin.Properties.Reported["modules"])
                {
                    var module = new GatewayModule()
                    {
                        ModuleName = element.Key
                    };

                    if (element.Value.Contains("status"))
                        module.Status = element.Value["status"];

                    if (element.Value.Contains("version"))
                        module.Version = element.Value["version"];

                    list.Add(module);
                }

                return list;
            }
            else
            {
                return list;
            }
        }

        public static bool IsValidDevEUI(ulong value)
        {
            return value is not 0 and not 0xffff_ffff_ffff_ffff;
        }
    }
}
