// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Services
{
    public interface IConfigService
    {
        Task<IEnumerable<Configuration>> GetIoTEdgeConfigurations();

        Task<IEnumerable<Configuration>> GetDevicesConfigurations();

        Task RolloutDeviceConfiguration(string modelName, Dictionary<string, object> desiredProperties);

        Task<Configuration> GetConfigItem(string id);
    }
}