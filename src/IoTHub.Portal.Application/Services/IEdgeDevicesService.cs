// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;

    public interface IEdgeDevicesService
    {
        Task<PaginatedResultDto<IoTEdgeListItemDto>> GetEdgeDevicesPage(
            string? searchText = null,
            bool? searchStatus = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[]? orderBy = null,
            string? modelId = null,
            List<string>? labels = default);

        Task<IoTEdgeDeviceDto> GetEdgeDevice(string edgeDeviceId);

        Task<IoTEdgeDeviceDto> CreateEdgeDevice(IoTEdgeDeviceDto edgeDevice);

        Task<IoTEdgeDeviceDto> UpdateEdgeDevice(IoTEdgeDeviceDto edgeDevice);

        Task DeleteEdgeDeviceAsync(string deviceId);

        Task<C2DresultDto> ExecuteModuleMethod(string deviceId, string moduleName, string methodName);

        Task<C2DresultDto> ExecuteModuleCommand(string deviceId, string moduleName, string commandName);

        Task<IEnumerable<LabelDto>> GetAvailableLabels();

        Task<string> GetEdgeDeviceEnrollementScript(string deviceId, string templateName);
    }
}
