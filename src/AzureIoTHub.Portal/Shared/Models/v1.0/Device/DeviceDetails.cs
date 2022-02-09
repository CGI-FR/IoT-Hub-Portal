// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.Device
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class DeviceDetails
    {
        [Required(ErrorMessage = "The device should have a unique identifier.")]
        public string DeviceID { get; set; }

        [Required(ErrorMessage = "The device should have a name.")]
        public string DeviceName { get; set; }

        [Required(ErrorMessage = "The device should use a model.")]
        public string ModelId { get; set; }

        public string ImageUrl { get; set; }

        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime StatusUpdatedTime { get; set; }

        public string LocationCode { get; set; }

        public string AssetId { get; set; }
    }
}
