// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models
{
    using System.Text.Json.Serialization;

#pragma warning disable CA1720 // Identifier contains type name
    /// <summary>
    /// Device property type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DevicePropertyType
    {
        /// <summary>
        /// Boolean property type.
        /// </summary>
        Boolean = 0,
        /// <summary>
        /// Double property type.
        /// </summary>
        Double = 1,
        /// <summary>
        /// Float property type.
        /// </summary>
        Float = 2,
        /// <summary>
        /// Integer property type.
        /// </summary>
        Integer = 3,
        /// <summary>
        /// Long property type.
        /// </summary>
        Long = 4,
        /// <summary>
        /// String property type.
        /// </summary>
        String = 5
    }
#pragma warning restore CA1720 // Identifier contains type name
}
