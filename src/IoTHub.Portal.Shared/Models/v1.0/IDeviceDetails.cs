// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models
{
    public interface IDeviceDetails
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        string DeviceID { get; set; }

        /// <summary>
        /// The name of the device.
        /// </summary>
        string DeviceName { get; set; }

        /// <summary>
        /// The model identifier.
        /// </summary>
        string ModelId { get; set; }

        /// <summary>
        /// The model name.
        /// </summary>
        string ModelName { get; set; }

        /// <summary>
        /// The device model image Url.
        /// </summary>
        string Image { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </summary>
        bool IsConnected { get; set; }

        /// <summary>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// The status updated time.
        /// </summary>
        DateTime StatusUpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets the last activity time.
        /// </summary>
        DateTime LastActivityTime { get; set; }

        /// <summary>
        /// List of custom device tags and their values.
        /// </summary>
        Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        List<LabelDto> Labels { get; set; }

        /// <summary>
        /// The LayerId of the device.
        /// </summary>
        string? LayerId { get; set; }
    }
}
