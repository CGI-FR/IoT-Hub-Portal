// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class DeviceModel
    {
        /// <summary>
        /// The device model identifier.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The device model image URL.
        /// </summary>
        public string ImageUrl { get; set; }

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
        /// The device OTAA Application eui.
        /// </summary>
        [Required(ErrorMessage = "The OTAA App EUI is required.")]
        public string AppEUI { get; set; }

        /// <summary>
        /// The sensor decoder URL.
        /// </summary>
        public string SensorDecoderURL { get; set; }

        /// <summary>A
        /// A value indicating whether this instance is builtin.
        /// </summary>
        public bool IsBuiltin { get; set; }

        /// <summary>
        /// The commands.
        /// </summary>
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
