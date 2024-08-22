// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0.LoRaWAN
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// LoRaWAN Device Class.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
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
