// Copyright (c) CGI France. All rights reserved.
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
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [ApiController]
    [Route("api/[controller]")]
    public class DeviceModelsController : ControllerBase
    {
        private const string DefaultPartitionKey = "0";

        private readonly ITableClientFactory tableClientFactory;
        private readonly IDeviceModelMapper deviceModelMapper;
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IDeviceService devicesService;

        public DeviceModelsController(
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceModelCommandMapper deviceModelCommandMapper,
            IDeviceModelMapper deviceModelMapper,
            IDeviceService devicesService,
            ITableClientFactory tableClientFactory)
        {
            this.deviceModelMapper = deviceModelMapper;
            this.tableClientFactory = tableClientFactory;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
            this.deviceModelImageManager = deviceModelImageManager;
            this.devicesService = devicesService;
        }

        /// <summary>
        /// Gets a list of device models from an Azure DataTable.
        /// </summary>
        /// <returns>A list of DeviceModel.</returns>
        [HttpGet]
        public IEnumerable<DeviceModel> Get()
        {
            // PartitionKey 0 contains all device models
            var entities = this.tableClientFactory
                            .GetDeviceTemplates()
                            .Query<TableEntity>();

            // Converts the query result into a list of device models
            var deviceModelList = entities.Select(this.deviceModelMapper.CreateDeviceModel);

            return deviceModelList;
        }

        /// <summary>
        /// Get a specific device model from an Azure DataTable.
        /// </summary>
        /// <returns>A DeviceModel.</returns>
        [HttpGet("{modelID}")]
        public IActionResult Get(string modelID)
        {
            var query = this.tableClientFactory
                            .GetDeviceTemplates()
                            .Query<TableEntity>(t => t.RowKey == modelID);

            if (!query.Any())
            {
                return this.NotFound();
            }

            return this.Ok(this.deviceModelMapper.CreateDeviceModel(query.Single()));
        }

        [HttpGet("{modelID}/image")]
        public Uri GetImage(string modelID)
        {
            return this.deviceModelImageManager.ComputeImageUri(modelID);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] string deviceModel, [FromForm] IFormFile file = null)
        {
            try
            {
                DeviceModel deviceModelObject = JsonConvert.DeserializeObject<DeviceModel>(deviceModel);
                TableEntity entity = new TableEntity()
                {
                    PartitionKey = DefaultPartitionKey,
                    RowKey = Guid.NewGuid().ToString()
                };

                await this.SaveEntity(entity, deviceModelObject, file);

                return this.Ok();
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromForm] string deviceModel, [FromForm] IFormFile file = null)
        {
            try
            {
                DeviceModel deviceModelObject = JsonConvert.DeserializeObject<DeviceModel>(deviceModel);

                TableEntity entity = new TableEntity()
                {
                    PartitionKey = DefaultPartitionKey,
                    RowKey = deviceModelObject.ModelId
                };

                await this.SaveEntity(entity, deviceModelObject, file);

                return this.Ok();
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [HttpDelete("{deviceModelID}")]
        public async Task<IActionResult> Delete(string deviceModelID)
        {
            // we get all devices
            var deviceList = await this.devicesService.GetAllDevice();
            // we get the device model with a query
            var query = this.tableClientFactory
                            .GetDeviceTemplates()
                            .Query<TableEntity>(t => t.RowKey == deviceModelID);

            if (!query.Any())
            {
                return this.NotFound();
            }

            var deviceModel = this.deviceModelMapper.CreateDeviceModel(query.Single());

            var queryCommand = this.tableClientFactory
                                   .GetDeviceCommands()
                                   .Query<TableEntity>(t => t.PartitionKey == deviceModel.ModelId);

            foreach (var twin in deviceList)
            {
                if (DeviceHelper.RetrieveTagValue(twin, "modelId") == deviceModel.ModelId)
                {
                    return this.Unauthorized("This model is already in use by a device and cannot be deleted.");
                }
            }

            var commands = queryCommand.Select(item => this.deviceModelCommandMapper.GetDeviceModelCommand(item)).ToList();

            // if we have command
            if (commands.Count > 0)
            {
                foreach (var item in commands)
                {
                    _ = await this.tableClientFactory
                        .GetDeviceCommands().DeleteEntityAsync(deviceModelID, item.CommandId);
                }
            }

            // Image deletion
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelID);

            var result = await this.tableClientFactory
                .GetDeviceTemplates()
                .DeleteEntityAsync(DefaultPartitionKey, deviceModelID);

            return this.StatusCode(result.Status);
        }

        private async Task SaveEntity(TableEntity entity, DeviceModel deviceModelObject, [FromForm] IFormFile file = null)
        {
            this.deviceModelMapper.UpdateTableEntity(entity, deviceModelObject);

            await this.tableClientFactory
                .GetDeviceTemplates()
                .UpsertEntityAsync(entity);

            if (file != null)
            {
                using var fileStream = file.OpenReadStream();
                await this.deviceModelImageManager.ChangeDeviceModelImageAsync(entity.RowKey, fileStream);
            }

            // insertion des commant
            if (deviceModelObject.Commands.Count > 0)
            {
                foreach (var element in deviceModelObject.Commands)
                {
                    TableEntity commandEntity = new TableEntity()
                    {
                        PartitionKey = entity.RowKey,
                        RowKey = element.Name
                    };

                    this.deviceModelCommandMapper.UpdateTableEntity(commandEntity, element);

                    await this.tableClientFactory
                        .GetDeviceCommands()
                        .AddEntityAsync(commandEntity);
                }
            }
        }
    }
}
