// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Entities
{
    internal interface IDevice
    {
        /// <summary>
        /// The device Identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The device friendly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The device Model Identifier.
        /// </summary>
        public string DeviceModelId { get; set; }

        /// <summary>
        /// A value indicating whether the device is currently connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// A value indicating whether the device is enabled on the platform.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The device last status updated time.
        /// </summary>
        public DateTime StatusUpdatedTime { get; set; }

        /// <summary>
        /// The device labels.
        /// </summary>
        public ICollection<Label> Labels { get; set; }
    }
}
