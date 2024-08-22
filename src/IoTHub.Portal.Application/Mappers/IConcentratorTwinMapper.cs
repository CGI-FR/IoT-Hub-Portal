// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using Microsoft.Azure.Devices.Shared;
    using Shared.Models.v1._0.LoRaWAN;

    public interface IConcentratorTwinMapper
    {
        ConcentratorDto CreateDeviceDetails(Twin twin);

        void UpdateTwin(Twin twin, ConcentratorDto item);
    }
}
