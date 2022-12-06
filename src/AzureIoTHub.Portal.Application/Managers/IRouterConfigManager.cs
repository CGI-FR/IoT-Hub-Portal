// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Managers
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public interface IRouterConfigManager
    {
        Task<RouterConfig> GetRouterConfig(string loRaRegion);
    }
}
