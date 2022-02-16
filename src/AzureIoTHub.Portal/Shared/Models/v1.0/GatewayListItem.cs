// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    public class GatewayListItem
    {
        /// <summary>
        /// The device identifier.
        /// </summary>
        [Required(ErrorMessage = "The device identifier is required.")]
        public string DeviceId { get; set; }

        /// <summary>
        /// The device status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The device type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The device nbdevices.
        /// </summary>
        public int NbDevices { get; set; }
    }
}
