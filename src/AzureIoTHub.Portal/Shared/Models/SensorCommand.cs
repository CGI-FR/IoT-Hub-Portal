// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SensorCommand
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Trame { get; set; }

        [Required]
        public int Port { get; set; }
    }
}
