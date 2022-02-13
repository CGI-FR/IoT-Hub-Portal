// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.V10.Concentrator;

    public interface IRouterConfigManager
    {
        Task<RouterConfig> GetRouterConfig(string loRaRegion);
    }
}
