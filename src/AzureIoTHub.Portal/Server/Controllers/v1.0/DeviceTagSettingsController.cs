// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v10
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.v10.Device;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class DeviceTagSettingsController : ControllerBase
    {
        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device tag mapper.
        /// </summary>
        private readonly IDeviceTagMapper deviceTagMapper;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<DeviceTagSettingsController> log;
        public const string DefaultPartitionKey = "0";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTagSettingsController"/> class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="deviceTagMapper">The device tag mapper.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
        public DeviceTagSettingsController(ILogger<DeviceTagSettingsController> log, IDeviceTagMapper deviceTagMapper, ITableClientFactory tableClientFactory)
        {
            this.log = log;
            this.tableClientFactory = tableClientFactory;
            this.deviceTagMapper = deviceTagMapper;
        }

        /// <summary>
        /// Update the device tag settings to be used in the application.
        /// </summary>
        /// <param name="tags">List of tags.</param>
        /// <returns>The action result.</returns>
        [HttpPost(Name = "POST Set device tag settings")]
        public async Task<IActionResult> Post(List<DeviceTag> tags)
        {
            foreach (DeviceTag tag in tags)
            {
                TableEntity entity = new TableEntity()
                {
                    PartitionKey = DefaultPartitionKey,
                    RowKey = tag.Name
                };
                await this.SaveEntity(entity, tag);
            }
            return this.Ok();
        }

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="tag">The device tag</param>
        /// <returns></returns>
        private async Task SaveEntity(TableEntity entity, DeviceTag tag)
        {
            this.deviceTagMapper.UpdateTableEntity(entity, tag);
            await this.tableClientFactory
                .GetDeviceTagSettings()
                .UpsertEntityAsync(entity);
        }
    }
}
