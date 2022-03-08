// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel
{
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class LoRaDeviceModel : LoRaDeviceModelBase
    {
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
            this.IsOTAAsetting = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaDeviceModel"/> class.
        /// </summary>
        public LoRaDeviceModel()
        {
        }
    }
}
