// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.v10;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    public interface IExternalDeviceService
    {
        Task<Device> GetDevice(string deviceId);

        Task<Twin> GetDeviceTwin(string deviceId);

        Task<Twin> GetDeviceTwinWithModule(string deviceId);
        Task<Twin> GetDeviceTwinWithEdgeHubModule(string deviceId);

        Task<BulkRegistryOperationResult> CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin, DeviceStatus isEnabled);

        Task<Device> UpdateDevice(Device device);

        Task<Twin> UpdateDeviceTwin(Twin twin);

        Task<CloudToDeviceMethodResult> ExecuteC2DMethod(string deviceId, CloudToDeviceMethod method);

        Task<CloudToDeviceMethodResult> ExecuteCustomCommandC2DMethod(string deviceId, string moduleName, CloudToDeviceMethod method);

        Task DeleteDevice(string deviceId);

        Task<PaginationResult<Twin>> GetAllDevice(
            string continuationToken = null,
            string filterDeviceType = null,
            string excludeDeviceType = null,
            string searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            Dictionary<string, string> searchTags = null,
            int pageSize = 10);

        Task<PaginationResult<Twin>> GetAllEdgeDevice(
            string continuationToken = null,
            string searchText = null,
            bool? searchStatus = null,
            string searchType = null,
            int pageSize = 10);

        Task<IEnumerable<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule edgeModule);

        Task<int> GetDevicesCount();

        Task<int> GetConnectedDevicesCount();

        Task<int> GetEdgeDevicesCount();

        Task<int> GetConnectedEdgeDevicesCount();

        Task<int> GetConcentratorsCount();

        Task<EnrollmentCredentials> GetEnrollmentCredentials(string deviceId);

        Task<EnrollmentCredentials> GetEdgeDeviceCredentials(string edgeDeviceId);

        Task<ConfigItem> RetrieveLastConfiguration(Twin twin);

        Task<Twin> CreateNewTwinFromDeviceId(string deviceId);

        Task<List<string>> GetAllGatewayID();

        Task<IEnumerable<string>> GetDevicesToExport();
    }
}
