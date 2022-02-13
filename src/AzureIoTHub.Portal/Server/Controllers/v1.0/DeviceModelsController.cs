// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
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
    using AzureIoTHub.Portal.Shared.Models.V10;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/models")]
    [ApiExplorerSettings(GroupName = "Device Models")]
    public class DeviceModelsController : ControllerBase
    {
        /// <summary>
        /// The default partition key.
        /// </summary>
        private const string DefaultPartitionKey = "0";

        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device model mapper.
        /// </summary>
        private readonly IDeviceModelMapper deviceModelMapper;

        /// <summary>
        /// The device model command mapper.
        /// </summary>
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;

        /// <summary>
        /// The device model image manager.
        /// </summary>
        private readonly IDeviceModelImageManager deviceModelImageManager;

        /// <summary>
        /// The devices service.
        /// </summary>
        private readonly IDeviceService devicesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModelsController"/> class.
        /// </summary>
        /// <param name="deviceModelImageManager">The device model image manager.</param>
        /// <param name="deviceModelCommandMapper">The device model command mapper.</param>
        /// <param name="deviceModelMapper">The device model mapper.</param>
        /// <param name="devicesService">The devices service.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
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
        /// Gets the device models.
        /// </summary>
        /// <returns>The list of device models.</returns>
        /// <response code="200">An array containing the existing device models.</response>
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
        /// Gets the specified model identifier.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The corresponding model.</returns>
        /// <response code="200">The model.</response>
        /// <response code="404">If the corresponding entity doesn't exist.</response>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var query = this.tableClientFactory
                            .GetDeviceTemplates()
                            .Query<TableEntity>(t => t.RowKey == id);

            if (!query.Any())
            {
                return this.NotFound();
            }

            return this.Ok(this.deviceModelMapper.CreateDeviceModel(query.Single()));
        }

        /// <summary>
        /// Gets the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The avatar.</returns>
        /// <response code="200">The device model's avatar URL.</response>
        [HttpGet("{id}/avatar")]
        public string GetAvatar(string id)
        {
            return this.deviceModelImageManager.ComputeImageUri(id);
        }

        /// <summary>
        /// Changes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="file">The file.</param>
        /// <returns>The avatar.</returns>
        /// <response code="200">The new device model's avatar URL.</response>
        [HttpPost("{id}/avatar")]
        public async Task<string> ChangeAvatar(string id, IFormFile file)
        {
            return await this.deviceModelImageManager.ChangeDeviceModelImageAsync(id, file.OpenReadStream());
        }

        /// <summary>
        /// Deletes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        [HttpDelete("{id}/avatar")]
        public async Task DeleteAvatar(string id)
        {
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(id);
        }

        /// <summary>
        /// Creates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        /// <response code="200">If the model is created.</response>
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

        /// <summary>
        /// Updates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        /// <response code="200">If the device model is updated.</response>
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

        /// <summary>
        /// Deletes the specified device model.
        /// </summary>
        /// <param name="id">The device model identifier.</param>
        /// <returns>The action result.</returns>
        /// <response code="204">If the device model is deleted.</response>
        /// <response code="404">If the device model is not found.</response>
        /// <response code="400">If the device model is used by a device.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // we get all devices
            var deviceList = await this.devicesService.GetAllDevice();
            // we get the device model with a query
            var query = this.tableClientFactory
                            .GetDeviceTemplates()
                            .Query<TableEntity>(t => t.RowKey == id);

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
                return this.BadRequest("This model is already in use by a device and cannot be deleted.");
            }

            var commands = queryCommand.Select(item => this.deviceModelCommandMapper.GetDeviceModelCommand(item)).ToList();

            foreach (var item in commands)
            {
                _ = await this.tableClientFactory
                    .GetDeviceCommands().DeleteEntityAsync(id, item.Name);
            }

            // Image deletion
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(id);

            var result = await this.tableClientFactory
                .GetDeviceTemplates()
                .DeleteEntityAsync(DefaultPartitionKey, id);

            return this.NoContent();
        }

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="deviceModelObject">The device model object.</param>
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
