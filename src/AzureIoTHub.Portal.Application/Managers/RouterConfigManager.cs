// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Managers
{
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;

    public class RouterConfigManager : IRouterConfigManager
    {
        public async Task<RouterConfig> GetRouterConfig(string loRaRegion)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            using var resourceStream = currentAssembly.GetManifestResourceStream($"{currentAssembly.GetName().Name}.RouterConfigFiles.{loRaRegion}.json");

            if (resourceStream == null)
                return null;

            return await JsonSerializer.DeserializeAsync<RouterConfig>(resourceStream);
        }
    }
}
