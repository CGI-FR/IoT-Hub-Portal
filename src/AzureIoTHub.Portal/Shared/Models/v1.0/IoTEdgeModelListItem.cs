// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class IoTEdgeModelListItem
    {
        /// <summary>
        /// The device model identifier.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The IoT Edge device model name.
        /// </summary>
        [Required(ErrorMessage = "The IoT Edge device model name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// The device model description.
        /// </summary>
        public string Description { get; set; }

        public Uri ImageUrl { get; set; }
    }
}
