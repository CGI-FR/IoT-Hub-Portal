// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceModelCommand
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required(ErrorMessage = "The command name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the frame.
        /// </summary>
        /// <value>
        /// The frame.
        /// </value>
        [Required(ErrorMessage = "The frame is required.")]
        public string Frame { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        [Required(ErrorMessage = "The port number is required.")]
        [Range(1, 223, ErrorMessage = "The port number should be between 1 and 223.")]
        public int Port { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is builtin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is builtin; otherwise, <c>false</c>.
        /// </value>
        public bool IsBuiltin { get; set; }
    }
}
