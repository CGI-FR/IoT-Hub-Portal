// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// IoT Edge configuration
    /// </summary>
    public class ConfigItem
    {
        /// <summary>
        /// The IoT Edge configuration name.
        /// </summary>
        [Required(ErrorMessage = "The configuration model name is required.")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// The IoT Edge configuration creation date.
        /// </summary>
        public DateTime DateCreation { get; set; }

        /// <summary>
        /// The IoT Edge configuration status.
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigItem"/> class.
        /// </summary>
        public ConfigItem()
        {
            DateCreation = new DateTime(1999, 1, 1);
        }
    }
}
