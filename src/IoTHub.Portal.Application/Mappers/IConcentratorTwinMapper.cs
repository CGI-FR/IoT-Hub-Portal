// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.Azure.Devices.Shared;

    public interface IConcentratorTwinMapper
    {
        ConcentratorDto CreateDeviceDetails(Twin twin);

        void UpdateTwin(Twin twin, ConcentratorDto item);
    }
}
