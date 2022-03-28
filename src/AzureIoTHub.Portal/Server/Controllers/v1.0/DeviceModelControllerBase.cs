// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    public abstract class DeviceModelsControllerBase<TListItemModel, TModel> : ControllerBase
        where TListItemModel : DeviceModel
        where TModel : DeviceModel
    {
        /// <summary>
        /// The default partition key.
        /// </summary>
#pragma warning disable RCS1158 // Static member in generic type should use a type parameter.
        public const string DefaultPartitionKey = "0";
#pragma warning restore RCS1158 // Static member in generic type should use a type parameter.

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device model mapper.
        /// </summary>
        private readonly IDeviceModelMapper<TListItemModel, TModel> deviceModelMapper;

        /// <summary>
        /// The device model image manager.
        /// </summary>
        private readonly IDeviceModelImageManager deviceModelImageManager;

        /// <summary>
        /// The devices service.
        /// </summary>
        private readonly IDeviceService devicesService;

        /// <summary>
        /// The device provisioning service manager.
        /// </summary>
        private readonly IDeviceProvisioningServiceManager deviceProvisioningServiceManager;

        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly IConfigService configService;

        /// <summary>
        /// The device template filter.
        /// </summary>
        private readonly string filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModelsController"/> class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="deviceModelImageManager">The device model image manager.</param>
        /// <param name="deviceModelMapper">The device model mapper.</param>
        /// <param name="devicesService">The devices service.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
        /// <param name="deviceProvisioningServiceManager">The device provisioning service manager.</param>
        /// <param name="configService">The configuration service.</param>
        /// <param name="filter">The device template filter query string.</param>
        protected DeviceModelsControllerBase(
            ILogger log,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceModelMapper<TListItemModel, TModel> deviceModelMapper,
            IDeviceService devicesService,
            ITableClientFactory tableClientFactory,
            IDeviceProvisioningServiceManager deviceProvisioningServiceManager,
            IConfigService configService,
            string filter)
        {
            this.log = log;
            this.deviceModelMapper = deviceModelMapper;
            this.tableClientFactory = tableClientFactory;
            this.deviceModelImageManager = deviceModelImageManager;
            this.devicesService = devicesService;
            this.filter = filter;
            this.deviceProvisioningServiceManager = deviceProvisioningServiceManager;
            this.configService = configService;
        }

        /// <summary>
        /// Gets the device models.
        /// </summary>
        /// <returns>The list of device models.</returns>
        public virtual ActionResult<IEnumerable<TListItemModel>> GetItems()
        {
            // PartitionKey 0 contains all device models
            var entities = this.tableClientFactory
                            .GetDeviceTemplates()
                            .Query<TableEntity>(this.filter);

            // Converts the query result into a list of device models
            var deviceModelList = entities.Select(this.deviceModelMapper.CreateDeviceModelListItem);

            return Ok(deviceModelList.ToArray());
        }

        /// <summary>
        /// Gets the specified model identifier.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The corresponding model.</returns>
        public virtual async Task<ActionResult<TModel>> GetItem(string id)
        {
            try
            {
                var query = await this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntityAsync<TableEntity>(DefaultPartitionKey, id);

                return Ok(this.deviceModelMapper.CreateDeviceModel(query.Value));
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.Log(LogLevel.Error, e.Message, e);

                throw;
            }
        }

        /// <summary>
        /// Gets the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The avatar.</returns>
        public virtual async Task<ActionResult<string>> GetAvatar(string id)
        {
            try
            {
                _ = await this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntityAsync<TableEntity>(DefaultPartitionKey, id);

                return this.Ok(this.deviceModelImageManager.ComputeImageUri(id).ToString());
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.Log(LogLevel.Error, e.Message, e);

                throw;
            }
        }

        /// <summary>
        /// Changes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="file">The file.</param>
        /// <returns>The avatar.</returns>
        public virtual async Task<ActionResult<string>> ChangeAvatar(string id, IFormFile file)
        {
            try
            {
                _ = await this.tableClientFactory
                               .GetDeviceTemplates()
                               .GetEntityAsync<TableEntity>(DefaultPartitionKey, id);

                return Ok(await this.deviceModelImageManager.ChangeDeviceModelImageAsync(id, file?.OpenReadStream()));
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.Log(LogLevel.Error, e.Message, e);

                throw;
            }
        }

        /// <summary>
        /// Deletes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        public virtual async Task<IActionResult> DeleteAvatar(string id)
        {
            try
            {
                _ = await this.tableClientFactory
                               .GetDeviceTemplates()
                               .GetEntityAsync<TableEntity>(DefaultPartitionKey, id);

                await this.deviceModelImageManager.DeleteDeviceModelImageAsync(id);

                return NoContent();
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.Log(LogLevel.Error, e.Message, e);

                throw;
            }
        }

        /// <summary>
        /// Creates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        public virtual async Task<IActionResult> Post(TModel deviceModel)
        {
            if (!string.IsNullOrEmpty(deviceModel?.ModelId))
            {
                try
                {
                    _ = await this.tableClientFactory
                                   .GetDeviceTemplates()
                                   .GetEntityAsync<TableEntity>(DefaultPartitionKey, deviceModel.ModelId);

                    return BadRequest("Cannot create a device model with an existing id.");
                }
                catch (RequestFailedException e)
                {
                    if (e.Status != StatusCodes.Status404NotFound)
                    {
                        this.log.Log(LogLevel.Error, e.Message, e);

                        throw;
                    }
                }
            }

            var entity = new TableEntity()
            {
                PartitionKey = DefaultPartitionKey,
                RowKey = string.IsNullOrEmpty(deviceModel.ModelId) ? Guid.NewGuid().ToString() : deviceModel.ModelId
            };

            await SaveEntity(entity, deviceModel);

            return Ok();
        }

        /// <summary>
        /// Updates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        public virtual async Task<IActionResult> Put(TModel deviceModel)
        {
            if (string.IsNullOrEmpty(deviceModel?.ModelId))
            {
                return BadRequest("Should provide the model id.");
            }

            TableEntity entity;

            try
            {
                entity = await this.tableClientFactory
                               .GetDeviceTemplates()
                               .GetEntityAsync<TableEntity>(DefaultPartitionKey, deviceModel.ModelId);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.Log(LogLevel.Error, e.Message, e);

                throw;
            }

            await SaveEntity(entity, deviceModel);

            return Ok();
        }

        /// <summary>
        /// Deletes the specified device model.
        /// </summary>
        /// <param name="id">The device model identifier.</param>
        /// <returns>The action result.</returns>
        public virtual async Task<IActionResult> Delete(string id)
        {
            // we get all devices
            var deviceList = await this.devicesService.GetAllDevice();

            try
            {
                _ = await this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntityAsync<TableEntity>(DefaultPartitionKey, id);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.Log(LogLevel.Error, e.Message, e);

                throw;
            }

            if (deviceList.Items.Any(x => DeviceHelper.RetrieveTagValue(x, "modelId") == id))
            {
                return BadRequest("This model is already in use by a device and cannot be deleted.");
            }

            var queryCommand = this.tableClientFactory
                                   .GetDeviceCommands()
                                   .Query<TableEntity>(t => t.PartitionKey == id)
                                   .ToArray();

            foreach (var item in queryCommand)
            {
                _ = await this.tableClientFactory
                                .GetDeviceCommands()
                                .DeleteEntityAsync(id, item.RowKey);
            }

            // Image deletion
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(id);

            _ = await this.tableClientFactory
                .GetDeviceTemplates()
                .DeleteEntityAsync(DefaultPartitionKey, id);

            return NoContent();
        }

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="deviceModelObject">The device model object.</param>
        private async Task SaveEntity(TableEntity entity, TModel deviceModelObject)
        {
            var desiredProperties = this.deviceModelMapper.UpdateTableEntity(entity, deviceModelObject);

            _ = await this.tableClientFactory
                .GetDeviceTemplates()
                .UpsertEntityAsync(entity);

            var deviceModelTwin = new TwinCollection();

            _ = await this.deviceProvisioningServiceManager.CreateEnrollmentGroupFromModelAsync(deviceModelObject.ModelId, deviceModelObject.Name, deviceModelTwin);

            await this.configService.RolloutDeviceConfiguration(deviceModelObject.ModelId, desiredProperties);
        }
    }
}
