// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Entities;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Models.v10;
    using Exceptions;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public abstract class DeviceModelPropertiesControllerBase : ControllerBase
    {
        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// The mapper.
        /// </summary>
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the Device model properties controller base class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tableClientFactory">the table client factory.</param>
        protected DeviceModelPropertiesControllerBase(
            ILogger log,
            IMapper mapper,
            ITableClientFactory tableClientFactory)
        {
            this.log = log;

            this.mapper = mapper;
            this.tableClientFactory = tableClientFactory;
        }

        /// <summary>
        /// Gets the device model properties.
        /// </summary>
        /// <param name="id">The device model properties</param>
        public virtual async Task<ActionResult<IEnumerable<DeviceProperty>>> GetProperties(string id)
        {
            if (!await DeviceModelExists(id))
            {
                return NotFound();
            }

            AsyncPageable<DeviceModelProperty> items;

            try
            {
                items = this.tableClientFactory
                    .GetDeviceTemplateProperties()
                    .QueryAsync<DeviceModelProperty>($"PartitionKey eq '{id}'");
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get existing device model properties for device with id {id}", e);
            }

            var result = new List<DeviceProperty>();

            await foreach (var item in items)
            {
                result.Add(this.mapper.Map<DeviceProperty>(item));
            }

            return Ok(result);
        }

        /// <summary>
        /// Sets the device model properties.
        /// </summary>
        /// <param name="id">The device model properties</param>
        /// <param name="properties">The model properties</param>
        public virtual async Task<ActionResult> SetProperties(string id, IEnumerable<DeviceProperty> properties)
        {
            if (!(await DeviceModelExists(id)))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            ArgumentNullException.ThrowIfNull(properties, nameof(properties));

            var table = this.tableClientFactory
                         .GetDeviceTemplateProperties();

            AsyncPageable<DeviceModelProperty> items;

            try
            {
                items = table
                    .QueryAsync<DeviceModelProperty>($"PartitionKey eq '{id}'");
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get existing device model properties for device with id {id}", e);
            }

            await foreach (var item in items)
            {
                try
                {
                    _ = await table.DeleteEntityAsync(id, item.RowKey);
                }
                catch (RequestFailedException e)
                {
                    throw new InternalServerErrorException($"Unable to delete the property {item.RowKey} for device model with id {id}", e);
                }
            }

            foreach (var item in properties)
            {
                var entity = this.mapper.Map<DeviceModelProperty>(item, opts => opts.Items[nameof(DeviceModelProperty.PartitionKey)] = id);

                try
                {
                    _ = await table.AddEntityAsync(entity);
                }
                catch (RequestFailedException e)
                {
                    throw new InternalServerErrorException($"Unable to add the property {item.Name} for device model with id {id}", e);
                }
            }

            return Ok();
        }

        private async Task<bool> DeviceModelExists(string id)
        {
            try
            {
                _ = await this.tableClientFactory
                                    .GetDeviceTemplates()
                                    .GetEntityAsync<TableEntity>("0", id);

                return true;
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    return false;
                }

                this.log.LogError(e.Message, e);

                throw new InternalServerErrorException($"Unable to check if device model with id {id} exist", e);
            }
        }
    }
}
