// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    public interface IEdgeDevicesService
    {
        PaginationResult<IoTEdgeListItem> GetEdgeDevicesPage(PaginationResult<Twin> edgeDevicesTwin,
            IUrlHelper urlHelper,
            string searchText = null,
            bool? searchStatus = null,
            string searchType = null,
            int pageSize = 10);

        Task<IoTEdgeDevice> GetEdgeDevice(string edgeDeviceId);

        Task<BulkRegistryOperationResult> CreateEdgeDevice(IoTEdgeDevice edgeDevice);

        Task<Twin> UpdateEdgeDevice(IoTEdgeDevice edgeDevice);

        Task<C2Dresult> ExecuteModuleMethod(IoTEdgeModule edgeModule, string deviceId, string methodName);

        Task<EnrollmentCredentials> GetEdgeDeviceCredentials(string edgeDeviceId);
    }
}
