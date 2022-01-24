// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.Device
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DeviceDetails
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only Alphabets and Numbers allowed.")]
        public string DeviceID { get; set; }

        public Uri ImageUrl { get; set; }

        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime LastActivityDate { get; set; }

        public string AppEUI { get; set; }

        public string AppKey { get; set; }

        public string LocationCode { get; set; }

        public string AssetID { get; set; }

        public string DeviceType { get; set; }

        public string ModelId { get; set; }

        [Required]
        public string ModelName { get; set; }

        public string SensorDecoder { get; set; }

        public List<Command> Commands { get; set; } = new List<Command>();
    }
}
