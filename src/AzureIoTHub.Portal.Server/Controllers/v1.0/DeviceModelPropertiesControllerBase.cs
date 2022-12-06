// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public abstract class DeviceModelPropertiesControllerBase : ControllerBase
    {
        /// <summary>
        /// The mapper.
        /// </summary>
        private readonly IMapper mapper;

        /// <summary>
        /// The device model properties services.
        /// </summary>
        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;

        /// <summary>
        /// Initializes a new instance of the Device model properties controller base class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="deviceModelPropertiesService">The device model properties services..</param>
        protected DeviceModelPropertiesControllerBase(
            IMapper mapper,
            IDeviceModelPropertiesService deviceModelPropertiesService)
        {
            this.mapper = mapper;
            this.deviceModelPropertiesService = deviceModelPropertiesService;
        }

        /// <summary>
        /// Gets the device model properties.
        /// </summary>
        /// <param name="id">The device model properties</param>
        public virtual async Task<ActionResult<IEnumerable<DeviceProperty>>> GetProperties(string id)
        {
            var result = new List<DeviceProperty>();

            try
            {
                foreach (var item in await this.deviceModelPropertiesService.GetModelProperties(id))
                {
                    result.Add(this.mapper.Map<DeviceProperty>(item));
                }

                return Ok(result);
            }
            catch (ResourceNotFoundException e)
            {
                return this.NotFound(e.Message);
            }
        }

        /// <summary>
        /// Sets the device model properties.
        /// </summary>
        /// <param name="id">The device model properties</param>
        /// <param name="properties">The model properties</param>
        public virtual async Task<ActionResult> SetProperties(string id, IEnumerable<DeviceProperty> properties)
        {
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

                await this.deviceModelPropertiesService.SavePropertiesForModel(id, entities);

                return Ok();
            }
            catch (ResourceNotFoundException e)
            {
                return this.NotFound(e.Message);
            }
        }
    }
}
