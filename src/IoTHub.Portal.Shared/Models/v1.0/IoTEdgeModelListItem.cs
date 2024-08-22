// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class IoTEdgeModelListItem
    {
        /// <summary>
        /// The device model identifier.
        /// </summary>
        public string ModelId { get; set; } = default!;

        /// <summary>
        /// The IoT Edge device model name.
        /// </summary>
        [Required(ErrorMessage = "The IoT Edge device model name is required.")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// The device model description.
        /// </summary>
        public string Description { get; set; } = default!;

        public Uri ImageUrl { get; set; } = default!;

        /// <summary>
        /// The aws deployment ID.
        /// </summary>
        public string ExternalIdentifier { get; set; } = default!;


        /// <summary>
        /// Gets the edge model labels.
        /// </summary>
        public IEnumerable<LabelDto> Labels { get; set; } = new List<LabelDto>();
    }
}
