// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Device tag.
    /// </summary>
    public class DeviceTagDto
    {
        /// <summary>
        /// The registered name in the device twin.
        /// </summary>
        [Required(ErrorMessage = "The tag should have a name.")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Name may only contain alphanumeric characters")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// The label shown to the user.
        /// </summary>
        [Required(ErrorMessage = "The tag should have a label.")]
        public string Label { get; set; } = default!;

        /// <summary>
        /// Whether the field is required when creating a new device or not.
        /// Default is false.
        /// </summary>
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Required { get; set; }

        /// <summary>
        /// Whether the field can be searcheable via the device search panel or not.
        /// Default is false.
        /// </summary>
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Searchable { get; set; }
    }
}
