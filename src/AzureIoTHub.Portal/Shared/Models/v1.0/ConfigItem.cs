// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    public class ConfigItem
    {
        /// <summary>
        /// The device model name.
        /// </summary>
        [Required(ErrorMessage = "The device model name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// The device dateCreation.
        /// </summary>
        public DateTime DateCreation { get; set; }

        /// <summary>
        /// The device status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The command configItem.
        /// </summary>
        public ConfigItem()
        {
            this.DateCreation = new DateTime(1999, 1, 1);
        }
    }
}
