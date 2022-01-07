// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
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
            try
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
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
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
