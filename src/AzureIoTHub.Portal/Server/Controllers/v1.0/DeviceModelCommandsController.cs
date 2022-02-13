// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.V10;
    using Microsoft.AspNetCore.Mvc;

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
        /// Initializes a new instance of the <see cref="DeviceModelCommandsController"/> class.
        /// </summary>
        /// <param name="deviceModelCommandMapper">The device model command mapper.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
        public DeviceModelCommandsController(
            IDeviceModelCommandMapper deviceModelCommandMapper,
            ITableClientFactory tableClientFactory)
        {
            this.tableClientFactory = tableClientFactory;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
        }

        /// <summary>
        /// Updates the specified device model's command.
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        /// <param name="command">The command.</param>
        /// <returns>The action result.</returns>
        /// <response code="200">If te device model's command is updated.</response>
        [HttpPost]
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

            return this.Ok();
        }

        /// <summary>
        /// Deletes the specified device model's command.
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        /// <param name="commandId">The command identifier.</param>
        /// <returns>The action result.</returns>
        /// <response code="204">If the device model's command is deleted.</response>
        [HttpDelete("{commandId}")]
        public async Task<IActionResult> Delete(string modelId, string commandId)
        {
            var result = await this.tableClientFactory.GetDeviceCommands().DeleteEntityAsync(modelId, commandId);
            return this.NoContent();
        }
    }
}
