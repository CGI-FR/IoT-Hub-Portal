// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Domain.Entities
{
    public class Concentrator : EntityBase
    {
        /// <summary>
        /// The name of the device.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// The lora region.
        /// </summary>
        [Required]
        public string LoraRegion { get; set; } = default!;

        /// <summary>
        /// The type of the device.
        /// </summary>
        public string DeviceType { get; set; } = default!;

        /// <summary>
        /// The client certificate thumbprint.
        /// </summary>
        public string? ClientThumbprint { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </summary>
        public bool IsEnabled { get; set; }

        public int Version { get; set; }
    }
}
