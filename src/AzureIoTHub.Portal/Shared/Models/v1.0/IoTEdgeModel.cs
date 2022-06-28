// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class IoTEdgeModel : IoTEdgeModelListItem
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

        /// <summary>
        /// The device model module list.
        /// </summary>
        public List<IoTEdgeModule> EdgeModules { get; set; } = new List<IoTEdgeModule>();
    }
}
