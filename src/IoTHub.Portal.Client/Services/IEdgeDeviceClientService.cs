// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IEdgeDeviceClientService
    {
        Task<PaginationResult<IoTEdgeListItemDto>> GetDevices(string continuationUri);

        Task<IoTEdgeDeviceDto> GetDevice(string deviceId);

        Task CreateDevice(IoTEdgeDeviceDto device);

        Task UpdateDevice(IoTEdgeDeviceDto device);

        Task DeleteDevice(string deviceId);

        Task<DeviceCredentialsDto> GetEnrollmentCredentials(string deviceId);

        Task<string> GetEnrollmentScriptUrl(string deviceId, string templateName);

        Task<List<IoTEdgeDeviceLogDto>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModuleDto edgeModule);

        Task<C2DresultDto> ExecuteModuleMethod(string deviceId, string moduleName, string methodName);

        Task<IEnumerable<LabelDto>> GetAvailableLabels();

    }
}
