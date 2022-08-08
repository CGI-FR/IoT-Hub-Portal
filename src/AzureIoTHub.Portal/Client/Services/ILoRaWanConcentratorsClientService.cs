// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.v10.LoRaWAN;
    using Portal.Models.v10.LoRaWAN;

    public interface ILoRaWanConcentratorsClientService
    {
        Task<PaginationResult<Concentrator>> GetConcentrators(string continuationUri);

        Task<Concentrator> GetConcentrator(string deviceId);

        Task CreateConcentrator(Concentrator concentrator);

        Task UpdateConcentrator(Concentrator concentrator);

        Task DeleteConcentrator(string deviceId);

        Task<IEnumerable<FrequencyPlan>> GetFrequencyPlans();
    }
}
