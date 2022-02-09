// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class LoRaDeviceModel : DeviceModel.DeviceModel
    {
        /// <summary>
        /// The device OTAA Application eui.
        /// </summary>
        [Required(ErrorMessage = "The OTAA App EUI is required.")]
        public string AppEUI { get; set; }

        /// <summary>
        /// The sensor decoder URL.
        /// </summary>
        public string SensorDecoderURL { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaDeviceModel"/> class.
        /// </summary>
        /// <param name="from">The device model taht the LoRa Device model should herit.</param>
        public LoRaDeviceModel(DeviceModel.DeviceModel from)
        {
            this.ModelId = from.ModelId;
            this.Name = from.Name;
            this.Description = from.Description;
            this.IsBuiltin = from.IsBuiltin;
            this.ImageUrl = from.ImageUrl;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaDeviceModel"/> class.
        /// </summary>
        public LoRaDeviceModel()
        {
        }
    }
}
