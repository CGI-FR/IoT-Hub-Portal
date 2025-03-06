// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Services
{
    public interface ILoRaWANConcentratorService
    {
        Task<PaginatedResult<ConcentratorDto>> GetAllDeviceConcentrator(ConcentratorFilter concentratorFilter);
        Task<ConcentratorDto> GetConcentrator(string deviceId);
        Task<ConcentratorDto> CreateDeviceAsync(ConcentratorDto concentrator);
        Task<ConcentratorDto> UpdateDeviceAsync(ConcentratorDto concentrator);
        Task DeleteDeviceAsync(string deviceId);
    }
}
