// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/edge/devices")]
    [ApiExplorerSettings(GroupName = "IoT Edge Devices")]
    public class EdgeDevicesController : ControllerBase
    {
        /// <summary>
        /// The device  edge devices controller.
        /// </summary>
        private readonly ILogger<EdgeDevicesController> logger;

        /// <summary>
        /// The device idevice service.
        /// </summary>
        private readonly IExternalDeviceService externalDevicesService;

        /// <summary>
        /// The edge device service.
        /// </summary>
        private readonly IEdgeDevicesService edgeDevicesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDevicesController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="service">The device service.</param>
        /// <param name="edgeDevicesService">The edge deviceservice.</param>
        public EdgeDevicesController(
            ILogger<EdgeDevicesController> logger,
            IExternalDeviceService service,
            IEdgeDevicesService edgeDevicesService)
        {
            this.edgeDevicesService = edgeDevicesService;
            this.logger = logger;
            this.externalDevicesService = service;
        }

        /// <summary>
        /// Gets the IoT Edge device list.
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="searchStatus"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="orderBy"></param>
        /// <param name="modelId"></param>
        [HttpGet(Name = "GET IoT Edge devices")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResult<IoTEdgeListItem>))]
        public async Task<PaginationResult<IoTEdgeListItem>> Get(
            string searchText = null,
            bool? searchStatus = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null,
            string modelId = null,
            [FromQuery] string[] labels = null)
        {
            var paginatedEdgeDevices = await this.edgeDevicesService.GetEdgeDevicesPage(
                searchText,
                searchStatus,
                pageSize,
                pageNumber,
                orderBy,
                modelId,
                labels?.ToList());

            var nextPage = string.Empty;

            if (paginatedEdgeDevices.HasNextPage)
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    RouteName = "GET IoT Edge devices",
                    Values = new
                    {
                        searchText,
                        searchStatus,
                        pageSize,
                        pageNumber = pageNumber + 1,
                        orderBy
                    }
                });
            }

            return new PaginationResult<IoTEdgeListItem>
            {
                Items = paginatedEdgeDevices.Data,
                TotalItems = paginatedEdgeDevices.TotalCount,
                NextPage = nextPage
            };
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        [HttpGet("{deviceId}", Name = "GET IoT Edge device")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IoTEdgeDevice))]
        public async Task<IActionResult> Get(string deviceId)
        {
            try
            {
                return Ok(await this.edgeDevicesService.GetEdgeDevice(deviceId));
            }
            catch (DeviceNotFoundException e)
            {
                return StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
        }

        /// <summary>
        /// Creates the IoT Edge device.
        /// </summary>
        /// <param name="edgeDevice">The IoT Edge device.</param>
        [HttpPost(Name = "POST Create IoT Edge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEdgeDeviceAsync(IoTEdgeDevice edgeDevice)
        {
            return Ok(await this.edgeDevicesService.CreateEdgeDevice(edgeDevice));
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="edgeDevice">The IoT Edge device.</param>
        [HttpPut("{deviceId}", Name = "PUT Update IoT Edge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDeviceAsync(IoTEdgeDevice edgeDevice)
        {
            return Ok(await this.edgeDevicesService.UpdateEdgeDevice(edgeDevice));
        }

        /// <summary>
        /// Deletes the device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        [HttpDelete("{deviceId}", Name = "DELETE Remove IoT Edge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteDeviceAsync(string deviceId)
        {
            await this.edgeDevicesService.DeleteEdgeDeviceAsync(deviceId);
            this.logger.LogInformation($"iot hub device was delete  {deviceId}");

            return Ok($"iot hub device was delete  {deviceId}");
        }

        /// <summary>
        /// Gets the IoT Edge device enrollement credentials.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        [HttpGet("{deviceId}/credentials", Name = "GET Device enrollment credentials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<EnrollmentCredentials>> GetCredentials(string deviceId)
        {
            try
            {
                return Ok(await this.externalDevicesService.GetEdgeDeviceCredentials(deviceId));
            }
            catch (ResourceNotFoundException e)
            {
                return NotFound(e);
            }
        }

        /// <summary>
        /// Executes the module method on the IoT Edge device.
        /// </summary>
        /// <param name="moduleName">The edge module name.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="methodName">Name of the method.</param>
        [HttpPost("{deviceId}/{moduleName}/{methodName}", Name = "POST Execute module command")]
        public async Task<C2Dresult> ExecuteModuleMethod(string deviceId, string moduleName, string methodName)
        {
            return await this.edgeDevicesService.ExecuteModuleMethod(deviceId, moduleName, methodName);
        }

        /// <summary>
        /// Get edge device logs
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="edgeModule">Edge module</param>
        /// <returns></returns>
        [HttpPost("{deviceId}/logs", Name = "Get Edge Device logs")]
        public async Task<IEnumerable<IoTEdgeDeviceLog>> GetEdgeDeviceLogs(string deviceId, IoTEdgeModule edgeModule)
        {
            ArgumentNullException.ThrowIfNull(edgeModule, nameof(edgeModule));

            return await this.externalDevicesService.GetEdgeDeviceLogs(deviceId, edgeModule);
        }

        [HttpGet("available-labels", Name = "GET Available Labels on Edge Devices")]
        public Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            return this.edgeDevicesService.GetAvailableLabels();
        }
    }
}
