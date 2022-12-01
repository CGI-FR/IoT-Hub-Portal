// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Models.v10;

    public interface IEdgeModuleCommandsService
    {
        Task<IEnumerable<EdgeModuleCommand>> GetAllEdgeModule(string edgeModelId);
        Task SaveEdgeModuleCommandAsync(string edgeModelId, List<IoTEdgeModule> edgeModules);
        Task ExecuteModuleCommand(string deviceId, string commandId);
    }
}
