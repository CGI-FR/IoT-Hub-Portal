// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;

    public interface IConfigs
    {
        Task<IEnumerable<Configuration>> GetAllConfigs();

        Task<Configuration> GetConfigItem(string id);
    }
}
