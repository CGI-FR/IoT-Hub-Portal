// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    public interface IEdgeDeviceMapper
    {
        IoTEdgeListItem CreateEdgeDeviceListItem(Twin deviceTwin);

        IoTEdgeDevice CreateEdgeDevice(Twin deviceTwin, Twin deviceTwinWithModules, int nbConnectedDevice, ConfigItem lastConfiguration, IEnumerable<string> tags);
    }
}
