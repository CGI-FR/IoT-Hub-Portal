// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;

    public interface IDeviceService
    {
        Task<IEnumerable<Twin>> GetAllEdgeDeviceWithTags();

        Task<IEnumerable<Twin>> GetAllEdgeDevice();

        Task<Device> GetDevice(string deviceId);

        Task<Twin> GetDeviceTwin(string deviceId);

        Task<Twin> GetDeviceTwinWithModule(string deviceId);

        Task<AttestationMechanism> GetDpsAttestionMechanism();

        Task<BulkRegistryOperationResult> CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin, DeviceStatus isEnabled);

        Task<Device> UpdateDevice(Device device);

        Task<Twin> UpdateDeviceTwin(string deviceId, Twin twin);

        Task<CloudToDeviceMethodResult> ExecuteC2DMethod(string deviceId, CloudToDeviceMethod method);

        Task DeleteDevice(string deviceId);

        Task<IEnumerable<Twin>> GetAllDevice();
    }
}
