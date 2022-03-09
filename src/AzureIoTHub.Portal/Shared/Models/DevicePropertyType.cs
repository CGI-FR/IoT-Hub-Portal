// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System.Text.Json.Serialization;

#pragma warning disable CA1720 // Identifier contains type name
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DevicePropertyType
    {
        Boolean,
        Double,
        Float,
        Integer,
        Long,
        String
    }
#pragma warning restore CA1720 // Identifier contains type name
}
