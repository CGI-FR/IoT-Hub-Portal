// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Portal.Shared;
    using Portal.Shared.Models.v1._0;

    public interface IDeviceClientService
    {
        Task<PaginationResult<DeviceListItem>> GetDevices(string continuationUri);

        Task<DeviceDetails> GetDevice(string deviceId);

        Task<string> CreateDevice(DeviceDetails device);

        Task UpdateDevice(DeviceDetails device);

        Task<IList<DevicePropertyValue>> GetDeviceProperties(string deviceId);

        Task SetDeviceProperties(string deviceId, IList<DevicePropertyValue> deviceProperties);

        Task<DeviceCredentials> GetEnrollmentCredentials(string deviceId);

        Task DeleteDevice(string deviceId);

        Task<HttpContent> ExportDeviceList();

        Task<HttpContent> ExportTemplateFile();

        Task<ImportResultLine[]> ImportDeviceList(MultipartFormDataContent dataContent);

        Task<IEnumerable<LabelDto>> GetAvailableLabels();
    }
}
