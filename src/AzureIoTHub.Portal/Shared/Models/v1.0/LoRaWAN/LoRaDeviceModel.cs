// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10.LoRaWAN
{
    using System;

    /// <summary>
    /// LoRa Device model.
    /// </summary>
    public class LoRaDeviceModel : LoRaDeviceModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaDeviceModel"/> class.
        /// </summary>
        /// <param name="from">The device model taht the LoRa Device model should herit.</param>
        public LoRaDeviceModel(DeviceModel from)
        {
            ArgumentNullException.ThrowIfNull(from, nameof(from));

            ModelId = from.ModelId;
            Name = from.Name;
            Description = from.Description;
            IsBuiltin = from.IsBuiltin;
            ImageUrl = from.ImageUrl;
            UseOTAA = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaDeviceModel"/> class.
        /// </summary>
        public LoRaDeviceModel()
        {
        }
    }
}
