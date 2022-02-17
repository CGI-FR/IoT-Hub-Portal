// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator;
    using Microsoft.Azure.Devices.Shared;

    public interface IConcentratorTwinMapper
    {
        Concentrator CreateDeviceDetails(Twin twin);

        void UpdateTwin(Twin twin, Concentrator item);
    }
}
