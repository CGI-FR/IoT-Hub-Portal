// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using AzureIoTHub.Portal.Domain.Base;

    public class EdgeDevice : EntityBase
    {
        public string Name { get; set; }

        public string DeviceModelId { get; set; }

        public int Version { get; set; }

        public string ConnectionState { get; set; }


        /// <summary>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The IoT Edge scope tag value.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// The number of connected devices on IoT Edge device.
        /// </summary>
        public int NbDevices { get; set; }

        /// <summary>
        /// The number of modules on IoT Edge device.
        /// </summary>
        public int NbModules { get; set; }

        /// <summary>
        /// List of custom device tags and their values.
        /// </summary>
        public ICollection<DeviceTagValue> Tags { get; set; }

        /// <summary>
        /// The device model
        /// </summary>
        public EdgeDeviceModel DeviceModel { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        public ICollection<Label> Labels { get; set; }
    }
}
