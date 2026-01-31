// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    public interface IConcentratorTwinMapper
    {
        ConcentratorDto CreateDeviceDetails(Twin twin);

        void UpdateTwin(Twin twin, ConcentratorDto item);
    }
}
