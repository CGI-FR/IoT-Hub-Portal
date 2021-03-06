// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/models/{id}/commands")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANCommandsController : ControllerBase
    {
        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device model command mapper.
        /// </summary>
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<LoRaWANCommandsController> log;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaWANCommandsController"/> class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="deviceModelCommandMapper">The device model command mapper.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
        public LoRaWANCommandsController(
            ILogger<LoRaWANCommandsController> log,
            IDeviceModelCommandMapper deviceModelCommandMapper,
            ITableClientFactory tableClientFactory)
        {
            this.log = log;
            this.tableClientFactory = tableClientFactory;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
        }

        /// <summary>
        /// Updates the device model's commands.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="commands">The commands.</param>
        /// <returns>The action result.</returns>
        [HttpPost(Name = "POST Set device model commands")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Post(string id, DeviceModelCommand[] commands)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(commands, nameof(commands));

            try
            {
                _ = await this.tableClientFactory
                             .GetDeviceTemplates()
                             .GetEntityAsync<TableEntity>(LoRaWANDeviceModelsController.DefaultPartitionKey, id);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.LogError(e.Message, e);

                throw;
            }

            Pageable<TableEntity> query;

            try
            {
                query = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"PartitionKey eq '{id}'");
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get existing commands of the model with id {id}", e);
            }

            foreach (var item in query)
            {
                try
                {
                    _ = await this.tableClientFactory
                        .GetDeviceCommands()
                        .DeleteEntityAsync(item.PartitionKey, item.RowKey);
                }
                catch (RequestFailedException e)
                {
                    throw new InternalServerErrorException($"Unable to delete the command {item.RowKey} of the model with id {id}", e);
                }
            }

            foreach (var command in commands)
            {
                var entity = new TableEntity()
                {
                    PartitionKey = id,
                    RowKey = command.Name
                };

                this.deviceModelCommandMapper.UpdateTableEntity(entity, command);

                try
                {
                    _ = await this.tableClientFactory
                        .GetDeviceCommands()
                        .AddEntityAsync(entity);
                }
                catch (RequestFailedException e)
                {
                    throw new InternalServerErrorException($"Unable to create the command {command.Name} of the model with id {id}", e);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Gets the device model's commands.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The action result.</returns>
        [HttpGet(Name = "GET Device model commands")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceModelCommand[]>> Get(string id)
        {
            try
            {
                _ = await this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntityAsync<TableEntity>(LoRaWANDeviceModelsController.DefaultPartitionKey, id);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.LogError(e.Message, e);

                throw;
            }

            Pageable<TableEntity> query;

            try
            {
                query = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"PartitionKey eq '{id}'");
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get commands of the model with id {id}", e);
            }

            var commands = new List<DeviceModelCommand>();

            foreach (var item in query)
            {
                var command = this.deviceModelCommandMapper.GetDeviceModelCommand(item);
                commands.Add(command);
            }

            return Ok(commands.ToArray());
        }
    }
}
