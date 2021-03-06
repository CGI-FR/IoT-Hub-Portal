// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.Azure.Devices.Shared;

    public interface IConcentratorTwinMapper
    {
        Concentrator CreateDeviceDetails(Twin twin);

        void UpdateTwin(Twin twin, Concentrator item);
    }
}
