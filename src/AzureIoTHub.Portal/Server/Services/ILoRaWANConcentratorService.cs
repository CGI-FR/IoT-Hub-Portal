// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices.Shared;

    public interface ILoRaWANConcentratorService
    {
        Task<bool> CreateDeviceAsync(Concentrator device);
        Task<bool> UpdateDeviceAsync(Concentrator device);
        PaginationResult<Concentrator> GetAllDeviceConcentrator(PaginationResult<Twin> twinResults, IUrlHelper urlHelper);
    }
}
