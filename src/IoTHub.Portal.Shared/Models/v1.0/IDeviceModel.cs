// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models
{
    public interface IDeviceModel
    {
        /// <summary>
        /// The device model identifier.
        /// </summary>
        string ModelId { get; set; }

        /// <summary>
        /// The device model image Url.
        /// </summary>
        string Image { get; set; }

        /// <summary>
        /// The device model name.
        /// </summary>
        [Required(ErrorMessage = "The device model name is required.")]
        string Name { get; set; }

        /// <summary>
        /// The device model description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// A value indicating whether this instance is builtin.
        /// </summary>
        bool IsBuiltin { get; set; }

        /// <summary>
        /// A value indicating whether the device model supports LoRa features.
        /// </summary>
        bool SupportLoRaFeatures { get; }

        /// <summary>
        /// Labels
        /// </summary>
        List<LabelDto> Labels { get; set; }
    }
}
