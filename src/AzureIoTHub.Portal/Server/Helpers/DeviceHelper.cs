// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using AzureIoTHub.Portal.Server.Extensions;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json.Linq;

    public static class DeviceHelper
    {
        /// <summary>
        /// This function generates the symmetricKey of a device
        /// from its Id.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="attestation">the enrollment group attestation.</param>
        /// <returns>The device symmetric key.</returns>
        public static string RetrieveSymmetricKey(string deviceId, SymmetricKeyAttestation attestation)
        {
            ArgumentNullException.ThrowIfNull(attestation, nameof(attestation));

            using var hmac = new HMACSHA256();
            hmac.Key = Convert.FromBase64String(attestation.PrimaryKey);
            var sig = hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId));

            return Convert.ToBase64String(sig);
        }

        /// <summary>
        /// Checks if the specific tag exists within the device twin,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="item">the device twin.</param>
        /// <param name="tagName">the tag property.</param>
        /// <returns>The corresponding tag value, or null if it doesn't exist.</returns>
        public static string RetrieveTagValue(Twin item, string tagName)
        {
            ArgumentNullException.ThrowIfNull(item, nameof(item));

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
            ArgumentNullException.ThrowIfNull(item, nameof(item));
            ArgumentNullException.ThrowIfNull(tagName, nameof(tagName));

            var camelCasedTagName = tagName.ToCamelCase();

            item.Tags[camelCasedTagName] = value;
        }

        /// <summary>
        /// Checks if the specific property exists within the device twin,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="twin">Device twin.</param>
        /// <param name="propertyName">Property to retrieve.</param>
        /// <returns>The corresponding property value, or null if it doesn't exist.</returns>
        public static string RetrieveDesiredPropertyValue(Twin twin, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            return twin.Properties.Desired.Contains(propertyName) ?
                twin.Properties.Desired[propertyName].ToString() : null;
        }

        /// <summary>
        /// Set the desired property value.
        /// </summary>
        /// <param name="twin">Device twin.</param>
        /// <param name="propertyName">Property to set.</param>
        /// <param name="value">Property value.</param>
        public static void SetDesiredProperty(Twin twin, string propertyName, object value)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            twin.Properties.Desired[propertyName] = value;
        }

        /// <summary>
        /// Set the ClientThumbprint Property as an array of string.
        /// </summary>
        /// <param name="twin">Device twin.</param>
        /// <param name="propertyName">Property to set (ClientThumbprint).</param>
        /// <param name="value">Property value.</param>
        public static void SetClientThumbprintProperty(Twin twin, string propertyName, string value)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            var camelCasedPropertyName = propertyName.ToCamelCase();
            twin.Properties.Desired[camelCasedPropertyName] = new List<string>() { value };
        }

        /// <summary>
        /// Checks if the specific property exists within the device twin,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="twin">Device twin.</param>
        /// <param name="propertyName">Property name (ClientThumbprint)</param>
        /// <returns>Corresponding property value, or null if it doesn't exist.</returns>
        public static string RetrieveClientThumbprintPropertyValue(Twin twin, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            var camelCasedPropertyName = propertyName.ToCamelCase();

            return twin.Properties.Desired.Contains(camelCasedPropertyName) ?
                twin.Properties.Desired[camelCasedPropertyName][0] : null;
        }

        /// <summary>
        /// Checks if the specific property exists within the device twin,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="twin">Device twin.</param>
        /// <param name="propertyName">Property to retrieve.</param>
        /// <returns>Corresponding property value, or null if it doesn't exist.</returns>
        public static string RetrieveReportedPropertyValue(Twin twin, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            return twin.Properties.Reported.Contains(propertyName) ?
                twin.Properties.Reported[propertyName] : null;
        }

        /// <summary>
        /// this function retrieve and return the number of connected
        /// devices.
        /// </summary>
        /// <param name="twin">the twin of the device.</param>
        /// <returns>the number of connected device.</returns>
        public static int RetrieveConnectedDeviceCount(Twin twin)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            return twin.Properties.Reported.Contains("clients") ?
                twin.Properties.Reported["clients"].Count : 0;
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
            ArgumentNullException.ThrowIfNull(deviceId, nameof(deviceId));
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            return twin.Properties.Desired.Contains("modules") && twin.DeviceId == deviceId
                ? twin.Properties.Desired["modules"].Count : 0;
        }

        /// <summary>
        /// This function get and return the runtime status of the module
        /// edgeAgent as the runtime response of the device.
        /// </summary>
        /// <param name="twin">the twin of the device we want.</param>
        /// <returns>string.</returns>
        public static string RetrieveRuntimeResponse(Twin twin)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            var reportedProperties = JObject.Parse(twin.Properties.Reported.ToJson());

            if (reportedProperties.TryGetValue("systemModules", out var systemModules)
                && systemModules.Value<JObject>().TryGetValue("edgeAgent", out var edgeAgentModule)
                && edgeAgentModule.Value<JObject>().TryGetValue("runtimeStatus", out var runtimeStatus))
            {
                return runtimeStatus.Value<string>();
            }

            return string.Empty;
        }

        /// <summary>
        /// This function get and return a list of the modules.
        /// </summary>
        /// <param name="twin">the twin of the device we want.</param>
        /// <returns> List of GatewayModule.</returns>
        public static IReadOnlyCollection<IoTEdgeModule> RetrieveModuleList(Twin twin)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            var list = new List<IoTEdgeModule>();
            var reportedProperties = JObject.Parse(twin.Properties.Reported.ToJson());

            if (!reportedProperties.TryGetValue("modules", out var modules))
            {
                return list;
            }

            foreach (var property in modules.Value<JObject>())
            {
                var propertyObject = property.Value.Value<JObject>();

                var module = new IoTEdgeModule()
                {
                    ModuleName = property.Key
                };

                if (propertyObject.TryGetValue("status", out var status))
                {
                    module.Status = status.Value<string>();
                }

                if (propertyObject.TryGetValue("version", out var version))
                {
                    module.Version = version.Value<string>();
                }

                list.Add(module);
            }

            return list.AsReadOnly();
        }
    }
}
