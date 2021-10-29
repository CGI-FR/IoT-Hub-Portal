// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class DeviceListItem
    {
        [Required]
        [StringLength(8, ErrorMessage ="Name lenght can't be more than 8.")]
        public string DeviceID { get; set; }

        public bool IsConnected { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime LastActivityDate { get; set; }

        public string AppEUI { get; set; }

        public string AppKey { get; set; }

        public string LocationCode { get; set; }

        public string AssetID { get; set; }

        [Required]
        public string DeviceType { get; set; }

        [Required]
        public string ModelType { get; set; }
    }
}
