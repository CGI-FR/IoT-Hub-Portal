// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Interfaces
{
    using System.Collections.Generic;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    public interface IDevicesService
    {
        IEnumerable<Twin> GetAllEdgeDeviceWithTags();

        IEnumerable<Twin> GetAllEdgeDevice();

        Twin GetDeviceTwin(string deviceId);

        Twin GetDeviceWithModule(string deviceId);

        BulkRegistryOperationResult CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin);

        Device UpdateDevice(Device device);

        Twin UpdateDeviceTwin(string deviceId, Twin twin);

        void Delete(string deviceId);
    }
}
