// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v1._0
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
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [Route("api/edge-models")]
    [ApiExplorerSettings(GroupName = "IoT Edge Devices Models")]
    [ApiController]
    public class EdgeDeviceModelsController : ControllerBase
    {
        /// <summary>
        /// The default partition key.
        /// </summary>
        public const string DefaultPartitionKey = "0";

        private readonly IConfigService configService;

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
        private readonly IEdgeDeviceModelMapper edgeDeviceModelMapper;

        public EdgeDeviceModelsController(IConfigService configService,
            ILogger log,
            ITableClientFactory tableClientFactory,
            IEdgeDeviceModelMapper edgeDeviceModelMapper)
        {
            this.configService = configService;
            this.log = log;
            this.tableClientFactory = tableClientFactory;
            this.edgeDeviceModelMapper = edgeDeviceModelMapper;
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

                return Ok(this.edgeDeviceModelMapper.CreateEdgeDeviceModelListItem(query.Value));
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
                        this.log.Log(LogLevel.Error, e.Message, e);

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
                    .GetDeviceTemplates()
                    .UpsertEntityAsync(entity);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to upsert the device model entity.", e);
            }

            var deviceModelTwin = new TwinCollection();

            _ = await this.deviceProvisioningServiceManager.CreateEnrollmentGroupFromModelAsync(deviceModelObject.ModelId, deviceModelObject.Name, deviceModelTwin);

            await this.configService.RollOutDeviceModelConfiguration(deviceModelObject.ModelId, desiredProperties);
        }
    }
}
}
