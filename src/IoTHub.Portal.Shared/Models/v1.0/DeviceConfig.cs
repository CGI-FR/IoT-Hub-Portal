// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10
{
    public class DeviceConfig
    {
        /// <summary>
        /// The configuration identifier.
        /// </summary>
        public string ConfigurationId { get; set; } = default!;

        /// <summary>
        /// The model identifier.
        /// </summary>
        public string ModelId { get; set; } = default!;

        /// <summary>
        /// The device tags targeted by the configuration.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();

        /// <summary>
        /// The configuration properties.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// The Configuration priority.
        /// </summary>
        [Range(0, Int32.MaxValue)]
        [DefaultValue(100)]
        public int Priority { get; set; } = 100;
    }
}
