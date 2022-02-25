// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v10
{
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.v10.Device;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("/api/settings/device-tags")]
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
            this.deviceTagMapper = deviceTagMapper;
            this.tableClientFactory = tableClientFactory;
        }

        /// <summary>
        /// Updates the device tag settings to be used in the application.
        /// </summary>
        /// <param name="tags">List of tags.</param>
        /// <returns>The action result.</returns>
        [HttpPost(Name = "POST a set of device tag settings")]
        public async Task<IActionResult> Post(List<DeviceTag> tags)
        {
            var query = this.tableClientFactory
                .GetDeviceTagSettings()
                .Query<TableEntity>()
                .AsPages();

            foreach (var page in query)
            {
                foreach (var item in page.Values)
                {
                    await this.tableClientFactory
                        .GetDeviceTagSettings()
                        .DeleteEntityAsync(item.PartitionKey, item.RowKey);
                }
            }

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
        /// Gets the device tag settings to be used in the application
        /// </summary>
        /// <returns>The list of tags</returns>
        [HttpGet(Name = "GET a set of device settings")]
        public ActionResult<List<DeviceTag>> Get()
        {
                var query = this.tableClientFactory
                    .GetDeviceTagSettings()
                    .Query<TableEntity>();

                var tagList = query.Select(this.deviceTagMapper.GetDeviceTag);

                return this.Ok(tagList.ToList());
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
                .AddEntityAsync(entity);
        }
    }
}
