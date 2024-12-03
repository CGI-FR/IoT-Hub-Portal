// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10.LoRaWAN
{
    /// <summary>
    /// LoRaWAN Deduplication strategy.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeduplicationMode
    {
        /// <summary>
        /// (default): allows duplicates to pass upstream without marking them.
        /// </summary>
        None = 0,
        /// <summary>
        /// Drops messages without further processing upstream nor downstream
        /// </summary>
        Drop = 1,
        /// <summary>
        /// Marks messages as duplicates but allows them upstream to IoTHub. The main use-case for this is to triangulate the location of sensors based on the signal strength.
        /// </summary>
        Mark = 2
    }
}
