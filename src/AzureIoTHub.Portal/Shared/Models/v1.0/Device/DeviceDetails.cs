// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.Device
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DeviceDetails
    {
        [Required]
        [RegularExpression("^[A-F0-9]{16}$", ErrorMessage = "DeviceID must contain 16 hexadecimal characters (numbers from 0 to 9 and/or letters from A to F)")]
        public string DeviceID { get; set; }

        [Required]
        public string DeviceName { get; set; }

        public string ImageUrl { get; set; }

        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime StatusUpdatedTime { get; set; }

        [Required]
        public string AppEUI { get; set; }

        public string AppKey { get; set; }

        public string LocationCode { get; set; }

        public string AssetId { get; set; }

        public string DeviceType { get; set; }

        public string ModelId { get; set; }

        [Required]
        public string ModelName { get; set; }

        public string SensorDecoder { get; set; }

        public bool AlreadyLoggedInOnce { get; set; }

        public List<Command> Commands { get; set; } = new List<Command>();
    }
}
