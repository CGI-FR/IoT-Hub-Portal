// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public interface ILoRaWanConcentratorClientService
    {
        Task<PaginationResult<ConcentratorDto>> GetConcentrators(string continuationUri);

        Task<ConcentratorDto> GetConcentrator(string deviceId);

        Task CreateConcentrator(ConcentratorDto concentrator);

        Task UpdateConcentrator(ConcentratorDto concentrator);

        Task DeleteConcentrator(string deviceId);

        Task<IEnumerable<FrequencyPlan>> GetFrequencyPlans();
    }
}
