// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;

    public interface IEdgeModuleCommandMethodManager
    {
        Task<HttpResponseMessage> ExecuteEdgeModuleCommandMessage(string deviceId, EdgeModuleCommandDto command);
    }
}
