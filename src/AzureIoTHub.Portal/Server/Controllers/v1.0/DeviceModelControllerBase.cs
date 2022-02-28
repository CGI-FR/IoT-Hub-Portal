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
    using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    public abstract class DeviceModelsControllerBase<TListItemModel, TModel> : ControllerBase
        where TModel : DeviceModel
        where TListItemModel : DeviceModel
    {
        /// <summary>
        /// The default partition key.
        /// </summary>
        public const string DefaultPartitionKey = "0";

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
        /// <param name="filter">The device template filter query string.</param>
        /// <param name="deviceProvisioningServiceManager">The device provisioning service manager.</param>
        /// <param name="configService">The configuration service.</param>
        public DeviceModelsControllerBase(
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
        public virtual ActionResult<IEnumerable<TListItemModel>> Get()
        {
            // PartitionKey 0 contains all device models
            var entities = this.tableClientFactory
                            .GetDeviceTemplates()
                            .Query<TableEntity>(filter);

            // Converts the query result into a list of device models
            var deviceModelList = entities.Select(this.deviceModelMapper.CreateDeviceModelListItem);

            return this.Ok(deviceModelList.ToArray());
        }

        /// <summary>
        /// Gets the specified model identifier.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The corresponding model.</returns>
        public virtual ActionResult<TModel> Get(string id)
        {
            try
            {
                var query = this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntity<TableEntity>(DefaultPartitionKey, id);

                return this.Ok(this.deviceModelMapper.CreateDeviceModel(query.Value));
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
        /// Gets the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The avatar.</returns>
        public virtual ActionResult<string> GetAvatar(string id)
        {
            try
            {
                var query = this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntity<TableEntity>(DefaultPartitionKey, id);

                return this.Ok(this.deviceModelImageManager.ComputeImageUri(id));
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
        /// Changes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="file">The file.</param>
        /// <returns>The avatar.</returns>
        public virtual async Task<ActionResult<string>> ChangeAvatar(string id, IFormFile file)
        {
            try
            {
                var query = this.tableClientFactory
                               .GetDeviceTemplates()
                               .GetEntity<TableEntity>(DefaultPartitionKey, id);

                return this.Ok(await this.deviceModelImageManager.ChangeDeviceModelImageAsync(id, file.OpenReadStream()));
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
        /// Deletes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        public virtual async Task<IActionResult> DeleteAvatar(string id)
        {
            try
            {
                var query = this.tableClientFactory
                               .GetDeviceTemplates()
                               .GetEntity<TableEntity>(DefaultPartitionKey, id);

                await this.deviceModelImageManager.DeleteDeviceModelImageAsync(id);

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

        /// <summary>
        /// Creates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        public virtual async Task<IActionResult> Post(TModel deviceModel)
        {
            if (!string.IsNullOrEmpty(deviceModel.ModelId))
            {
                try
                {
                    var query = this.tableClientFactory
                                   .GetDeviceTemplates()
                                   .GetEntity<TableEntity>(DefaultPartitionKey, deviceModel.ModelId);

                    return this.BadRequest("Cannot create a device model with an existing id.");
                }
                catch (RequestFailedException e)
                {
                    if (e.Status != StatusCodes.Status404NotFound)
                    {
                        this.log.LogError(e.Message, e);

                        throw;
                    }
                }
            }

            TableEntity entity = new TableEntity()
            {
                PartitionKey = DefaultPartitionKey,
                RowKey = string.IsNullOrEmpty(deviceModel.ModelId) ? Guid.NewGuid().ToString() : deviceModel.ModelId
            };

            await this.SaveEntity(entity, deviceModel);

            return this.Ok();
        }

        /// <summary>
        /// Updates the specified device model.
        /// </summary>
        /// <param name="deviceModel">The device model.</param>
        /// <returns>The action result.</returns>
        public virtual async Task<IActionResult> Put(TModel deviceModel)
        {
            if (string.IsNullOrEmpty(deviceModel.ModelId))
            {
                return this.BadRequest("Should provide the model id.");
            }

            TableEntity entity;

            try
            {
                entity = this.tableClientFactory
                               .GetDeviceTemplates()
                               .GetEntity<TableEntity>(DefaultPartitionKey, deviceModel.ModelId);
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

            await this.SaveEntity(entity, deviceModel);

            return this.Ok();
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
            TableEntity entity = null;

            try
            {
                entity = this.tableClientFactory
                            .GetDeviceTemplates()
                            .GetEntity<TableEntity>(DefaultPartitionKey, id);
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

            if (deviceList.Any(x => DeviceHelper.RetrieveTagValue(x, "modelId") == id))
            {
                return this.BadRequest("This model is already in use by a device and cannot be deleted.");
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
        private async Task SaveEntity(TableEntity entity, TModel deviceModelObject)
        {
            var desiredProperties = this.deviceModelMapper.UpdateTableEntity(entity, deviceModelObject);

            await this.tableClientFactory
                .GetDeviceTemplates()
                .UpsertEntityAsync(entity);

            var deviceModelTwin = new TwinCollection();

            await this.deviceProvisioningServiceManager.CreateEnrollmentGroupFormModelAsync(deviceModelObject.ModelId, deviceModelObject.Name, deviceModelTwin);

            await this.configService.RolloutDeviceConfiguration(deviceModelObject.Name, desiredProperties);
        }
    }
}