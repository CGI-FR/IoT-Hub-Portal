// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure.Messaging.EventHubs;
    using IoTHub.Portal.Shared.Models.v10;
    using Models.v10;
    using Shared.Models;
    using Shared.Models.v10;

    public interface IDeviceService<TDto>
        where TDto : IDeviceDetails
    {
        Task<PaginatedResult<DeviceListItem>> GetDevices(
            string? searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            int pageSize = 10,
            int pageNumber = 0,
            string[]? orderBy = null,
            Dictionary<string, string>? tags = default,
            string? modelId = null,
            List<string>? labels = default);

        Task<TDto> GetDevice(string deviceId);

        Task<bool> CheckIfDeviceExists(string deviceId);

        Task<TDto> CreateDevice(TDto device);

        Task<TDto> UpdateDevice(TDto device);

        Task DeleteDevice(string deviceId);

        Task<DeviceCredentials> GetCredentials(TDto device);

        Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId);

        Task ProcessTelemetryEvent(EventData eventMessage);

        Task<IEnumerable<LabelDto>> GetAvailableLabels();
    }
}
