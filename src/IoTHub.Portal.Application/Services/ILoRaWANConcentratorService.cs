// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Shared.Models.v1._0;
    using Shared.Models.v1._0.Filters;
    using Shared.Models.v1._0.LoRaWAN;

    public interface ILoRaWANConcentratorService
    {
        Task<PaginatedResult<ConcentratorDto>> GetAllDeviceConcentrator(ConcentratorFilter concentratorFilter);
        Task<ConcentratorDto> GetConcentrator(string deviceId);
        Task<ConcentratorDto> CreateDeviceAsync(ConcentratorDto concentrator);
        Task<ConcentratorDto> UpdateDeviceAsync(ConcentratorDto concentrator);
        Task DeleteDeviceAsync(string deviceId);
    }
}
