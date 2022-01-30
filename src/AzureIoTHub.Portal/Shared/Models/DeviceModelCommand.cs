// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceModelCommand
    {
        [Required(ErrorMessage = "The command name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The frame is required.")]
        public string Frame { get; set; }

        [Required(ErrorMessage = "The port number is required.")]
        [Range(1, 223, ErrorMessage = "The port number should be between 1 and 223.")]
        public int Port { get; set; } = 1;
    }
}
