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
    public class DeviceModelsController : ControllerBase
    {
        private const string DefaultPartitionKey = "0";

        private readonly ITableClientFactory tableClientFactory;
        private readonly ISensorImageManager sensorImageManager;
        private readonly ISensorCommandMapper sensorCommandMapper;

        public DeviceModelsController(
            ISensorImageManager sensorImageManager,
            ISensorCommandMapper sensorCommandMapper,
            ITableClientFactory tableClientFactory)
        {
            this.sensorImageManager = sensorImageManager;
            this.tableClientFactory = tableClientFactory;
            this.sensorCommandMapper = sensorCommandMapper;
        }

        /// <summary>
        /// Gets a list of sensor models from an Azure DataTable.
        /// </summary>
        /// <returns>A list of SensorModel.</returns>
        [HttpGet]
        public IEnumerable<SensorModel> Get()
        {
            // PartitionKey 0 contains all sensor models
            var entities = this.tableClientFactory
                            .GetDeviceTemplates()
                            .Query<TableEntity>();

            // Converts the query result into a list of sensor models
            var sensorsList = entities.Select(e => new SensorModel
            {
                ModelId = e.RowKey,
                ImageUrl = this.sensorImageManager.ComputeImageUri(e.RowKey).ToString(),
                Name = e[nameof(SensorModel.Name)]?.ToString(),
                Description = e[nameof(SensorModel.Description)]?.ToString(),
                AppEUI = e[nameof(SensorModel.AppEUI)]?.ToString()
            });

            return sensorsList;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] string sensor, [FromForm] IFormFile file = null)
        {
            try
            {
                SensorModel sensorObject = JsonConvert.DeserializeObject<SensorModel>(sensor);
                TableEntity entity = new TableEntity()
                {
                    PartitionKey = DefaultPartitionKey,
                    RowKey = Guid.NewGuid().ToString()
                };
                await this.SaveEntity(entity, sensorObject, file);

                return this.Ok();
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromForm] string sensor, [FromForm] IFormFile file = null)
        {
            try
            {
                SensorModel sensorObject = JsonConvert.DeserializeObject<SensorModel>(sensor);

                TableEntity entity = new TableEntity()
                {
                    PartitionKey = DefaultPartitionKey,
                    RowKey = sensorObject.ModelId
                };

                await this.SaveEntity(entity, sensorObject, file);

                return this.Ok();
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        private async Task SaveEntity(TableEntity entity, SensorModel sensorObject, [FromForm] IFormFile file = null)
        {
            entity[nameof(SensorModel.Name)] = sensorObject.Name;
            entity[nameof(SensorModel.Description)] = sensorObject.Description;
            entity[nameof(SensorModel.AppEUI)] = sensorObject.AppEUI;

            await this.tableClientFactory
                .GetDeviceTemplates()
                .AddEntityAsync(entity);

            using var fileStream = file.OpenReadStream();
            await this.sensorImageManager.ChangeSensorImageAsync(entity.RowKey, fileStream);

            // insertion des commant
            if (sensorObject.Commands.Count > 0)
            {
                foreach (var element in sensorObject.Commands)
                {
                    TableEntity commandEntity = new TableEntity()
                    {
                        PartitionKey = sensorObject.Name,
                        RowKey = element.Name
                    };

                    this.sensorCommandMapper.UpdateTableEntity(commandEntity, element);

                    await this.tableClientFactory
                        .GetDeviceCommands()
                        .AddEntityAsync(commandEntity);
                }
            }
        }
    }
}
