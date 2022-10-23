// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;
    using Portal.Models.v10.LoRaWAN;

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
