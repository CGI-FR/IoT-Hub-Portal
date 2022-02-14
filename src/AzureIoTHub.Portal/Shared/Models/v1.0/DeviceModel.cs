// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DeviceModel
    {
        /// <summary>
        /// Gets or sets the model identifier.
        /// </summary>
        /// <value>
        /// The model identifier.
        /// </value>
        public string ModelId { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        /// <value>
        /// The image URL.
        /// </value>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required(ErrorMessage = "The device model name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the application eui.
        /// </summary>
        /// <value>
        /// The application eui.
        /// </value>
        [Required(ErrorMessage = "The OTAA App EUI is required.")]
        public string AppEUI { get; set; }

        /// <summary>
        /// Gets or sets the sensor decoder URL.
        /// </summary>
        /// <value>
        /// The sensor decoder URL.
        /// </value>
        public string SensorDecoderURL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is builtin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is builtin; otherwise, <c>false</c>.
        /// </value>
        public bool IsBuiltin { get; set; }

        /// <summary>
        /// Gets or sets the commands.
        /// </summary>
        /// <value>
        /// The commands.
        /// </value>
        [ValidateComplexType]
        public List<DeviceModelCommand> Commands { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModel"/> class.
        /// </summary>
        public DeviceModel()
        {
            this.Commands = new List<DeviceModelCommand>();
        }
    }
}
