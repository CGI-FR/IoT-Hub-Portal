// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Shared.Models.v1._0;

    public interface ILoRaWANConcentratorService
    {
        Task<PaginatedResult<ConcentratorDto>> GetAllDeviceConcentrator(int pageSize = 10, int pageNumber = 0, string[] orderBy = null);
        Task<ConcentratorDto> GetConcentrator(string deviceId);
        Task<ConcentratorDto> CreateDeviceAsync(ConcentratorDto concentrator);
        Task<ConcentratorDto> UpdateDeviceAsync(ConcentratorDto concentrator);
        Task DeleteDeviceAsync(string deviceId);
    }
}
