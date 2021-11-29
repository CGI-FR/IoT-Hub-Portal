// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class SensorModel
    {
        public string ModelId { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string AppEUI { get; set; }

        public List<SensorCommand> Commands { get; set; }

        public SensorModel()
        {
            this.Commands = new List<SensorCommand>();
        }
    }
}
