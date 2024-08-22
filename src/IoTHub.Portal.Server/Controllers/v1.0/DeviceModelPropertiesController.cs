// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models.v1._0;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/models/{id}/properties")]
    [ApiExplorerSettings(GroupName = "Device Models")]
    public class DeviceModelPropertiesController : DeviceModelPropertiesControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the Device model properties controller class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="deviceModelPropertiesService">The device model properties service.</param>
        public DeviceModelPropertiesController(
            IMapper mapper,
            IDeviceModelPropertiesService deviceModelPropertiesService)
            : base(mapper, deviceModelPropertiesService)
        {
        }

        /// <summary>
        /// Gets the device model properties.
        /// </summary>
        /// <param name="id">The device model properties</param>
        [HttpGet(Name = "GET Device model properties")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DeviceProperty>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override async Task<ActionResult<IEnumerable<DeviceProperty>>> GetProperties(string id)
        {
            return await base.GetProperties(id);
        }

        /// <summary>
        /// Sets the device model properties.
        /// </summary>
        /// <param name="id">The device model properties</param>
        /// <param name="properties">The model properties</param>
        [HttpPost(Name = "POST Device model properties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override async Task<ActionResult> SetProperties(string id, IEnumerable<DeviceProperty> properties)
        {
            return await base.SetProperties(id, properties);
        }
    }
}
