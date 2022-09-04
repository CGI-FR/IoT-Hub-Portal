// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ITableClientFactory = Domain.ITableClientFactory;

    public abstract class DeviceModelPropertiesControllerBase : ControllerBase
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// The mapper.
        /// </summary>
        private readonly IMapper mapper;

        /// <summary>
        /// The unit of work.
        /// </summary>
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device model properties repository.
        /// </summary>
        private readonly IDeviceModelPropertiesRepository deviceModelPropertiesRepository;

        /// <summary>
        /// Initializes a new instance of the Device model properties controller base class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="log">The logger.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
        protected DeviceModelPropertiesControllerBase(
            IUnitOfWork unitOfWork,
            ILogger log,
            IMapper mapper,
            IDeviceModelPropertiesRepository deviceModelPropertiesRepository,
            ITableClientFactory tableClientFactory)
        {
            this.log = log;
            this.unitOfWork = unitOfWork;

            this.mapper = mapper;
            this.tableClientFactory = tableClientFactory;
            this.deviceModelPropertiesRepository = deviceModelPropertiesRepository;
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

            try
            {

                var result = new List<DeviceProperty>();

                foreach (var item in await this.deviceModelPropertiesRepository.GetModelProperties(id))
                {
                    result.Add(this.mapper.Map<DeviceProperty>(item));
                }

                return Ok(result);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get properties for model {id}", e);
            }
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

            try
            {
                var entities = properties.Select(item => this.mapper.Map<DeviceModelProperty>(item, opts => opts.Items[nameof(DeviceModelProperty.ModelId)] = id))
                    .ToArray();

                await this.deviceModelPropertiesRepository.SavePropertiesForModel(id, entities);

                await this.unitOfWork.SaveAsync();

                return Ok();
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to set properties for model {id}", e);
            }
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
