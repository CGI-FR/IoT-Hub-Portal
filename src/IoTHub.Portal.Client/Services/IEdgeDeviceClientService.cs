// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Portal.Shared;
    using Portal.Shared.Models.v1._0;

    public interface IEdgeDeviceClientService
    {
        Task<PaginationResult<IoTEdgeListItem>> GetDevices(string continuationUri);

        Task<IoTEdgeDevice> GetDevice(string deviceId);

        Task CreateDevice(IoTEdgeDevice device);

        Task UpdateDevice(IoTEdgeDevice device);

        Task DeleteDevice(string deviceId);

        Task<DeviceCredentials> GetEnrollmentCredentials(string deviceId);

        Task<string> GetEnrollmentScriptUrl(string deviceId, string templateName);

        Task<List<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule edgeModule);

        Task<C2Dresult> ExecuteModuleMethod(string deviceId, string moduleName, string methodName);

        Task<IEnumerable<LabelDto>> GetAvailableLabels();

    }
}
