// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceModelCommand
    {
        public string CommandId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Frame { get; set; }

        [Required]
        public int Port { get; set; }
    }
}
