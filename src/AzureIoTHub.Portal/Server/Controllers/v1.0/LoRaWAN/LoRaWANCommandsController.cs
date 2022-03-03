// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

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
            try
            {
                var templateQuery = this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntity<TableEntity>(LoRaWANDeviceModelsController.DefaultPartitionKey, id);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return this.NotFound();
                }

                this.log.LogError(e.Message, e);

                throw;
            }

            var query = this.tableClientFactory
                                  .GetDeviceCommands()
                                  .Query<TableEntity>(filter: $"PartitionKey eq '{id}'");

            foreach (var item in query)
            {
                await this.tableClientFactory
                               .GetDeviceCommands()
                               .DeleteEntityAsync(item.PartitionKey, item.RowKey);
            }

            foreach (var command in commands)
            {
                TableEntity entity = new TableEntity()
                {
                    PartitionKey = id,
                    RowKey = command.Name
                };

                this.deviceModelCommandMapper.UpdateTableEntity(entity, command);

                await this.tableClientFactory
                        .GetDeviceCommands()
                        .AddEntityAsync(entity);
            }

            return this.Ok();
        }

        /// <summary>
        /// Gets the device model's commands.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The action result.</returns>
        [HttpGet(Name = "GET Device model commands")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<DeviceModelCommand[]> Get(string id)
        {
            try
            {
                var templateQuery = this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntity<TableEntity>(LoRaWANDeviceModelsController.DefaultPartitionKey, id);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return this.NotFound();
                }

                this.log.LogError(e.Message, e);

                throw;
            }

            var query = this.tableClientFactory
                                  .GetDeviceCommands()
                                  .Query<TableEntity>(filter: $"PartitionKey  eq '{id}'");

            var commands = new List<DeviceModelCommand>();

            foreach (var item in query)
            {
                var command = this.deviceModelCommandMapper.GetDeviceModelCommand(item);
                commands.Add(command);
            }

            return this.Ok(commands);
        }
    }
}
