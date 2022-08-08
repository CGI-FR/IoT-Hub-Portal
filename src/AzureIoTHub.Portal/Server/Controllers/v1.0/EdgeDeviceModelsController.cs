// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Exceptions;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    //using Microsoft.Extensions.Logging;

    [Authorize]
    [Route("api/edge/models")]
    [ApiExplorerSettings(GroupName = "IoT Edge Devices Models")]
    [ApiController]
    public class EdgeDeviceModelsController : ControllerBase
    {
        /// <summary>
        /// The default partition key.
        /// </summary>
        public const string DefaultPartitionKey = "0";

        /// <summary>
        /// The Configurations service.
        /// </summary>
        private readonly IConfigService configService;

        private readonly IDeviceService deviceService;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<EdgeDeviceModelsController> log;

        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device model mapper.
        /// </summary>
        private readonly IEdgeDeviceModelMapper edgeDeviceModelMapper;

        /// <summary>
        /// The device model image manager.
        /// </summary>
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public EdgeDeviceModelsController(
            IConfigService configService,
            IDeviceService deviceService,
            ILogger<EdgeDeviceModelsController> log,
            ITableClientFactory tableClientFactory,
            IDeviceModelImageManager deviceModelImageManager,
            IEdgeDeviceModelMapper edgeDeviceModelMapper)
        {
            this.configService = configService;
            this.deviceService = deviceService;
            this.log = log;
            this.tableClientFactory = tableClientFactory;
            this.edgeDeviceModelMapper = edgeDeviceModelMapper;
            this.deviceModelImageManager = deviceModelImageManager;
        }

        [HttpGet]
        public ActionResult<List<IoTEdgeModelListItem>> GetEdgeModelList()
        {
            Pageable<TableEntity> entities;

            try
            {
                // PartitionKey 0 contains all device models
                entities = this.tableClientFactory
                            .GetEdgeDeviceTemplates()
                            .Query<TableEntity>();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get device models.", e);
            }

            var deviceModelList = entities.Select(this.edgeDeviceModelMapper.CreateEdgeDeviceModelListItem);

            return Ok(deviceModelList.ToArray());
        }

        [HttpGet("{modelId}")]
        public async Task<ActionResult<IoTEdgeModel>> GetEdgeDeviceModel(string modelId)
        {
            try
            {
                var query = await this.tableClientFactory
                            .GetEdgeDeviceTemplates()
                            .GetEntityAsync<TableEntity>(DefaultPartitionKey, modelId);

                var modules = new List<IoTEdgeModule>();

                var configs = await this.configService.GetIoTEdgeConfigurations();
                var configItem = configs.FirstOrDefault(x => x.Id.StartsWith(modelId));

                return Ok(this.edgeDeviceModelMapper.CreateEdgeDeviceModel(query.Value, modules));
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.Log(LogLevel.Error, e.Message, e);

                throw new InternalServerErrorException($"Unable to get the device model with the id: {modelId}", e);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdgeModel(IoTEdgeModel ioTEdgeModel)
        {
            if (!string.IsNullOrEmpty(ioTEdgeModel?.ModelId))
            {
                try
                {
                    _ = await this.tableClientFactory
                                   .GetEdgeDeviceTemplates()
                                   .GetEntityAsync<TableEntity>(DefaultPartitionKey, ioTEdgeModel.ModelId);

                    return BadRequest("Cannot create a device model with an existing id.");
                }
                catch (RequestFailedException e)
                {
                    if (e.Status != StatusCodes.Status404NotFound)
                    {
                        //this.log.Log(LogLevel.Error, e.Message, e);

                        throw new InternalServerErrorException($"Unable create the device model with id: {ioTEdgeModel?.ModelId}.", e);
                    }
                }
            }

            var entity = new TableEntity()
            {
                PartitionKey = DefaultPartitionKey,
                RowKey = string.IsNullOrEmpty(ioTEdgeModel.ModelId) ? Guid.NewGuid().ToString() : ioTEdgeModel.ModelId
            };

            await SaveEntity(entity, ioTEdgeModel);

            return Ok();
        }

        /// <summary>
        /// Delete the edge device model.
        /// </summary>
        /// <param name="modelId">the model id.</param>
        /// <returns>Http response</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpDelete("{modelId}")]
        public async Task<IActionResult> DeleteModelAsync(string modelId)
        {
            var deviceList = await this.deviceService.GetAllEdgeDevice();

            try
            {
                _ = await this.tableClientFactory
                            .GetEdgeDeviceTemplates()
                            .GetEntityAsync<TableEntity>(DefaultPartitionKey, modelId);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                this.log.Log(LogLevel.Error, e.Message, e);

                throw new InternalServerErrorException("Unable to get the device model entity.", e);
            }

            if (deviceList.Items.Any(x => DeviceHelper.RetrieveTagValue(x, "modelId") == modelId))
            {
                return BadRequest("This model is already in use by a device and cannot be deleted.");
            }

            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(modelId);

            try
            {
                _ = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .DeleteEntityAsync(DefaultPartitionKey, modelId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to delete the device model entity.", e);
            }

            return NoContent();
        }

        /// <summary>
        /// Gets the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The avatar.</returns>
        [HttpGet("{id}/avatar", Name = "GET edge Device model avatar URL")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<string>> GetAvatar(string id)
        {
            try
            {
                _ = await this.tableClientFactory
                            .GetEdgeDeviceTemplates()
                            .GetEntityAsync<TableEntity>(DefaultPartitionKey, id);

                return this.Ok(this.deviceModelImageManager.ComputeImageUri(id).ToString());
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                //this.log.Log(LogLevel.Error, e.Message, e);

                throw new InternalServerErrorException("Unable to get the device model avatar.", e);
            }
        }

        /// <summary>
        /// Changes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="file">The file.</param>
        /// <returns>The avatar.</returns>
        [HttpPost("{id}/avatar", Name = "POST Update the edge device model avatar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<string>> ChangeAvatar(string id, IFormFile file)
        {
            try
            {
                _ = await this.tableClientFactory
                               .GetEdgeDeviceTemplates()
                               .GetEntityAsync<TableEntity>(DefaultPartitionKey, id);

                return Ok(await this.deviceModelImageManager.ChangeDeviceModelImageAsync(id, file?.OpenReadStream()));
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                //this.log.Log(LogLevel.Error, e.Message, e);

                throw new InternalServerErrorException($"Unable to change the device model avatar with id:{id}.", e);
            }
        }

        /// <summary>
        /// Deletes the avatar.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        [HttpDelete("{id}/avatar", Name = "DELETE Remove the edge device model avatar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> DeleteAvatar(string id)
        {
            try
            {
                _ = await this.tableClientFactory
                               .GetEdgeDeviceTemplates()
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

                //this.log.Log(LogLevel.Error, e.Message, e);

                throw new InternalServerErrorException($"Unable to delete avatar of the device model with the id: {id}", e);
            }
        }

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="deviceModelObject">The device model object.</param>
        private async Task SaveEntity(TableEntity entity, IoTEdgeModel deviceModelObject)
        {
            var desiredProperties = this.edgeDeviceModelMapper.UpdateTableEntity(entity, deviceModelObject);

            try
            {
                _ = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .UpsertEntityAsync(entity);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to upsert the edge device model entity.", e);
            }

            await this.configService.RollOutEdgeModelConfiguration(deviceModelObject.ModelId, desiredProperties);
    }
}
}
