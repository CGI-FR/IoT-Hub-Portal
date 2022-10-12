// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v1._0;

    public interface IEdgeDevicesService
    {
        Task<PaginatedResult<IoTEdgeListItem>> GetEdgeDevicesPage(
            string searchText = null,
            bool? searchStatus = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null);

        Task<IoTEdgeDevice> GetEdgeDevice(string edgeDeviceId);

        Task<IoTEdgeDevice> CreateEdgeDevice(IoTEdgeDevice edgeDevice);

        Task<IoTEdgeDevice> UpdateEdgeDevice(IoTEdgeDevice edgeDevice);

        Task<C2Dresult> ExecuteModuleMethod(string deviceId, string moduleName, string methodName);

        Task<EnrollmentCredentials> GetEdgeDeviceCredentials(string edgeDeviceId);

        Task<C2Dresult> ExecuteModuleCommand(string deviceId, string moduleName, string commandName);
    }
}
