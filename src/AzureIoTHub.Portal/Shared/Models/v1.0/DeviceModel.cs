// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DeviceModel
    {
        public string ModelId { get; set; }

        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "The device model name is required.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "The OTAA App EUI is required.")]
        public string AppEUI { get; set; }

        public string SensorDecoderURL { get; set; }

        [ValidateComplexType]
        public List<DeviceModelCommand> Commands { get; set; }

        public DeviceModel()
        {
            this.Commands = new List<DeviceModelCommand>();
        }
    }
}
