// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    using AzureIoTHub.Portal.Domain.Base;

    public class Device : EntityBase
    {
        /// <summary>
        /// The name of the device.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The model identifier.
        /// </summary>
        public string DeviceModelId { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </summary>
        public bool IsEnabled { get; set; }

        public int Version { get; set; }

        /// <summary>
        /// The status updated time.
        /// </summary>
        public DateTime StatusUpdatedTime { get; set; }

        /// <summary>
        /// List of custom device tags and their values.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();
    }
}
