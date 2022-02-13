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

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/models")]
    [ApiExplorerSettings(GroupName = "Device Models")]
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

        [HttpGet("{modelID}/avatar")]
        public string GetAvatar(string modelID)
        {
            return this.deviceModelImageManager.ComputeImageUri(modelID);
        }

        [HttpPost("{modelID}/avatar")]
        public async Task<string> ChangeAvatar(string modelID, IFormFile file)
        {
            return await this.deviceModelImageManager.ChangeDeviceModelImageAsync(modelID, file.OpenReadStream());
        }

        [HttpDelete("{modelID}/avatar")]
        public async Task DeleteAvatar(string modelID)
        {
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(modelID);
        }

        [HttpPost]
        public async Task<IActionResult> Post(DeviceModel deviceModel)
        {
            TableEntity entity = new TableEntity()
            {
                PartitionKey = DefaultPartitionKey,
                RowKey = deviceModel.ModelId ?? Guid.NewGuid().ToString()
            };

            await this.SaveEntity(entity, deviceModel);

            return this.Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put(DeviceModel deviceModel)
        {
            TableEntity entity = new TableEntity()
            {
                PartitionKey = DefaultPartitionKey,
                RowKey = deviceModel.ModelId
            };

            await this.SaveEntity(entity, deviceModel);

            return this.Ok();
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

            if (deviceList.Any(x => DeviceHelper.RetrieveTagValue(x, "modelId") == deviceModel.ModelId))
            {
                return this.Unauthorized("This model is already in use by a device and cannot be deleted.");
            }

            var commands = queryCommand.Select(item => this.deviceModelCommandMapper.GetDeviceModelCommand(item)).ToList();

            foreach (var item in commands)
            {
                _ = await this.tableClientFactory
                    .GetDeviceCommands().DeleteEntityAsync(deviceModelID, item.Name);
            }

            // Image deletion
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelID);

            var result = await this.tableClientFactory
                .GetDeviceTemplates()
                .DeleteEntityAsync(DefaultPartitionKey, deviceModelID);

            return this.StatusCode(result.Status);
        }

        private async Task SaveEntity(TableEntity entity, DeviceModel deviceModelObject)
        {
            this.deviceModelMapper.UpdateTableEntity(entity, deviceModelObject);

            await this.tableClientFactory
                .GetDeviceTemplates()
                .UpsertEntityAsync(entity);

            var commandsTable = this.tableClientFactory.GetDeviceCommands();
            var commandsPage = commandsTable.QueryAsync<TableEntity>(c => c.PartitionKey == entity.RowKey)
                                            .AsPages();

            await foreach (var page in commandsPage)
            {
                foreach (var item in page.Values.Where(x => !deviceModelObject.Commands.Any(c => c.Name == x.RowKey)))
                {
                    await commandsTable.DeleteEntityAsync(item.PartitionKey, item.RowKey);
                }
            }

            foreach (var item in deviceModelObject.Commands)
            {
                TableEntity commandEntity = new TableEntity()
                {
                    PartitionKey = entity.RowKey,
                    RowKey = item.Name
                };

                this.deviceModelCommandMapper.UpdateTableEntity(commandEntity, item);

                await this.tableClientFactory
                    .GetDeviceCommands()
                    .UpsertEntityAsync(commandEntity);
            }
        }
    }
}
