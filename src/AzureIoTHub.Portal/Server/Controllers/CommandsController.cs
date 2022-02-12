// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly ITableClientFactory tableClientFactory;
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;

        public CommandsController(
            IDeviceModelCommandMapper deviceModelCommandMapper,
            ITableClientFactory tableClientFactory)
        {
            this.tableClientFactory = tableClientFactory;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
        }

        /// <summary>
        /// Add a command to an Azure DataTable.
        /// </summary>
        /// <returns>Operation status.</returns>
        [HttpPost("{modelId}")]
        public async Task<IActionResult> Post(string modelId, DeviceModelCommand command)
        {
            TableEntity entity = new TableEntity()
            {
                PartitionKey = modelId,
                RowKey = command.Name
            };
            this.deviceModelCommandMapper.UpdateTableEntity(entity, command);
            await this.tableClientFactory
                .GetDeviceCommands()
                .AddEntityAsync(entity);
            return this.Ok("Command successfully added");
        }

        /// <summary>
        /// Delete a command from an Azure DataTable.
        /// </summary>
        /// <returns>Operation status.</returns>
        [HttpDelete("{modelId}/{commandId}")]
        public async Task<IActionResult> Delete(string modelId, string commandId)
        {
            var result = await this.tableClientFactory.GetDeviceCommands().DeleteEntityAsync(modelId, commandId);
            return this.StatusCode(result.Status);
        }
    }
}
