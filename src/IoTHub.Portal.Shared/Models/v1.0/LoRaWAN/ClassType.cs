// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10.LoRaWAN
{
    /// <summary>
    /// LoRaWAN Device Class.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ClassType
    {
        /// <summary>
        /// Class A Device
        /// </summary>
        A = 0,
        /// <summary>
        /// Class C Device
        /// </summary>
        C = 1
    }
}
