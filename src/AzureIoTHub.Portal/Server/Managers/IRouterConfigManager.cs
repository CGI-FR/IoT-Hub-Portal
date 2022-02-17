// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator;
    using System.Threading.Tasks;

    public interface IRouterConfigManager
    {
        Task<RouterConfig> GetRouterConfig(string loRaRegion);
    }
}
