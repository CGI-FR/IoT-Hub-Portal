// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v10;
    using Portal.Models.v10;

    public interface IDeviceClientService
    {
        Task<PaginationResult<DeviceListItemDto>> GetDevices(string continuationUri);

        Task<DeviceDetailsDto> GetDevice(string deviceId);

        Task<string> CreateDevice(DeviceDetailsDto device);

        Task UpdateDevice(DeviceDetailsDto device);

        Task<IList<DevicePropertyValueDto>> GetDeviceProperties(string deviceId);

        Task SetDeviceProperties(string deviceId, IList<DevicePropertyValueDto> deviceProperties);

        Task<DeviceCredentialsDto> GetEnrollmentCredentials(string deviceId);

        Task DeleteDevice(string deviceId);

        Task<HttpContent> ExportDeviceList();

        Task<HttpContent> ExportTemplateFile();

        Task<ImportResultLineDto[]> ImportDeviceList(MultipartFormDataContent dataContent);

        Task<IEnumerable<LabelDto>> GetAvailableLabels();
    }
}
