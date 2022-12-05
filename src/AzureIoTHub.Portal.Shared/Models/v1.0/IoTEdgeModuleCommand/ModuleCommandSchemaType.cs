// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ModuleCommandSchemaType
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
        String = 5,
        /// <summary>
        /// 
        /// </summary>
        Date = 6,
        /// <summary>
        /// 
        /// </summary>
        DateTime = 7,
        /// <summary>
        /// 
        /// </summary>
        Time = 8,
        /// <summary>
        /// 
        /// </summary>
        Duration = 9,
        /// <summary>
        /// Complex type object.
        /// </summary>
        Object = 10,
        /// <summary>
        /// Complex type enum.
        /// </summary>
        Enum = 11,
    }
}
