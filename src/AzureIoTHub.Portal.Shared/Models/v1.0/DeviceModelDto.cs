// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Models.v10;

    /// <summary>
    /// Device model.
    /// </summary>
    public class DeviceModelDto : IDeviceModel
    {
        /// <summary>
        /// The device model identifier.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The device model image Url.
        /// </summary>
        public Uri ImageUrl { get; set; }

        /// <summary>
        /// The device model name.
        /// </summary>
        [Required(ErrorMessage = "The device model name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// The device model description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A value indicating whether this instance is builtin.
        /// </summary>
        public bool IsBuiltin { get; set; }

        /// <summary>
        /// A value indicating whether the LoRa features is supported on this model.
        /// </summary>
        public bool SupportLoRaFeatures { get; set; }

        /// <summary>
        /// Labels
        /// </summary>
        public List<LabelDto> Labels { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModelDto"/> class.
        /// </summary>
        public DeviceModelDto()
        {
        }
    }
}
