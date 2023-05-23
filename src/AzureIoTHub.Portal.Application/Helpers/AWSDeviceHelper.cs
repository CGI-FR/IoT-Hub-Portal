// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Helpers
{
    using System;
    using Amazon.IotData.Model;
    using Newtonsoft.Json;

    public static class AWSDeviceHelper
    {

        /// <summary>
        /// Checks if the specific property exists within the device shadow,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="shadow">Device shadow.</param>
        /// <param name="propertyName">Property to retrieve.</param>
        /// <returns>The corresponding property value, or null if it doesn't exist.</returns>
        public static string? RetrieveDesiredPropertyValue(GetThingShadowResponse shadow, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(shadow, nameof(shadow));

            var payloadString = System.Text.Encoding.UTF8.GetString(shadow.Payload.ToArray());
            var shadowDocument = JsonConvert.DeserializeObject<ShadowDocument>(payloadString);

            if (shadowDocument != null && shadowDocument.State?.Desired != null)
            {
                return shadowDocument.State.Desired.ContainsKey(propertyName) ?
                    shadowDocument.State.Desired[propertyName].ToString() : null;
            }

            return null;
        }

        /// <summary>
        /// Checks if the specific property exists within the device shadow,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="shadow">Device shadow.</param>
        /// <param name="propertyName">Property to retrieve.</param>
        /// <returns>Corresponding property value, or null if it doesn't exist.</returns>
        public static string? RetrieveReportedPropertyValue(GetThingShadowResponse shadow, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(shadow, nameof(shadow));

            var payloadString = System.Text.Encoding.UTF8.GetString(shadow.Payload.ToArray());
            var shadowDocument = JsonConvert.DeserializeObject<ShadowDocument>(payloadString);

            if (shadowDocument != null && shadowDocument.State?.Reported != null)
            {
                return shadowDocument.State.Reported.ContainsKey(propertyName) ?
                    shadowDocument.State.Reported[propertyName].ToString() : null;
            }

            return null;
        }

        /// <summary>
        /// Checks if the specific property exists within the device shadow,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="shadow">Device shadow.</param>
        /// <param name="propertyName">Property to retrieve.</param>
        /// <returns>The corresponding property value, or null if it doesn't exist.</returns>
        public static string? RetrieveDesiredProperties(GetThingShadowResponse shadow)
        {
            ArgumentNullException.ThrowIfNull(shadow, nameof(shadow));

            var payloadString = System.Text.Encoding.UTF8.GetString(shadow.Payload.ToArray());
            var shadowDocument = JsonConvert.DeserializeObject<ShadowDocument>(payloadString);

            if (shadowDocument != null && shadowDocument.State?.Desired != null)
            {
                return shadowDocument.State.Desired.ToString();
            }

            return null;
        }

        /// <summary>
        /// Checks if the specific property exists within the device shadow,
        /// Returns the corresponding value if so, else returns null.
        /// </summary>
        /// <param name="shadow">Device shadow.</param>
        /// <param name="propertyName">Property to retrieve.</param>
        /// <returns>Corresponding property value, or null if it doesn't exist.</returns>
        public static string? RetrieveReportedProperties(GetThingShadowResponse shadow)
        {
            ArgumentNullException.ThrowIfNull(shadow, nameof(shadow));

            var payloadString = System.Text.Encoding.UTF8.GetString(shadow.Payload.ToArray());
            var shadowDocument = JsonConvert.DeserializeObject<ShadowDocument>(payloadString);

            if (shadowDocument != null && shadowDocument.State?.Reported != null)
            {
                return shadowDocument.State.Reported.ToString();
            }

            return null;
        }

        private class ShadowDocument
        {
            [JsonProperty("state")]
            public ShadowState? State { get; set; }
        }

        private class ShadowState
        {
            [JsonProperty("desired")]
            public Dictionary<string, object>? Desired { get; set; }

            [JsonProperty("reported")]
            public Dictionary<string, object>? Reported { get; set; }
        }
    }
}
