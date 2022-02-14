// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.V10;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/models/{id}/commands")]
    [ApiExplorerSettings(GroupName = "Device Models")]
    public class DeviceModelCommandsController : ControllerBase
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
        private readonly ILogger<DeviceModelCommandsController> log;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModelCommandsController"/> class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="deviceModelCommandMapper">The device model command mapper.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
        public DeviceModelCommandsController(
            ILogger<DeviceModelCommandsController> log,
            IDeviceModelCommandMapper deviceModelCommandMapper,
            ITableClientFactory tableClientFactory)
        {
            this.log = log;
            this.tableClientFactory = tableClientFactory;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
        }

        /// <summary>
        /// Creates the specified device model's command.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="command">The command.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Post(string id, DeviceModelCommand command)
        {
            try
            {
                var query = this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntity<TableEntity>(DeviceModelsController.DefaultPartitionKey, id);

                TableEntity entity = new TableEntity()
                {
                    PartitionKey = id,
                    RowKey = command.Name
                };

                this.deviceModelCommandMapper.UpdateTableEntity(entity, command);

                await this.tableClientFactory
                        .GetDeviceCommands()
                        .AddEntityAsync(entity);

                return this.Ok();
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
        }

        /// <summary>
        /// Deletes the specified device model's command.
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        /// <param name="commandId">The command identifier.</param>
        /// <returns>The action result.</returns>
        /// <response code="204">If the device model's command is deleted.</response>
        [HttpDelete("{commandId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(string modelId, string commandId)
        {
            try
            {
                var query = this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntity<TableEntity>(DeviceModelsController.DefaultPartitionKey, modelId);

                var result = await this.tableClientFactory.GetDeviceCommands()
                                                .DeleteEntityAsync(modelId, commandId);

                return this.NoContent();
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
        }
    }
}
