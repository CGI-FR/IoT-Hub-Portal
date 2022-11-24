// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;

    public interface IEdgeModuleCommandsService
    {
        Task<EdgeModuleCommandDto[]> GetAllEdgeModule(string edgeModelId);
        Task SaveEdgeModuleCommandAsync(string edgeModelId, EdgeModuleCommandDto[] commands);
        Task ExecuteModuleCommand(string deviceId, string commandId);
    }
}
