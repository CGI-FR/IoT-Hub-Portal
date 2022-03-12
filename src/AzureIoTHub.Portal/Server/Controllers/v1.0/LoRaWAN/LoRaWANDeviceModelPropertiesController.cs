// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/models/{id}/properties")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANDeviceModelPropertiesController : DeviceModelPropertiesControllerBase
    {

        /// <summary>
        /// Initializes a new instance of the Device model properties controller class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tableClientFactory">the table client factory.</param>
        public LoRaWANDeviceModelPropertiesController(
            ILogger<LoRaWANDeviceModelPropertiesController> log,
            IMapper mapper,
            ITableClientFactory tableClientFactory)
            : base(log, mapper, tableClientFactory)
        {

        }

        /// <summary>
        /// Gets the device model properties.
        /// </summary>
        /// <param name="id">The device model properties</param>
        /// <returns></returns>
        [HttpGet(Name = "GET LoRaWAN Device model properties")]
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
        /// <returns></returns>
        [HttpPost(Name = "POST LoRaWAN Device model properties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public override async Task<ActionResult> SetProperties(string id, IEnumerable<DeviceProperty> properties)
        {
            return await base.SetProperties(id, properties);
        }
    }
}
