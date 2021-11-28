// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DeviceListItem
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

        public string ModelType { get; set; }

        public List<SensorCommand> Commands { get; set; }

        public DeviceListItem()
        {
            this.Commands = new List<SensorCommand>();
        }
    }
}
