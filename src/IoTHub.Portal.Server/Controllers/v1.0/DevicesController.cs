// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Shared;
    using Shared.Models.v1._0;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/devices")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class DevicesController : DevicesControllerBase<DeviceDetails>
    {
        private readonly IDevicePropertyService devicePropertyService;

        public DevicesController(
            ILogger<DevicesController> logger,
            IDevicePropertyService devicePropertyService,
            IDeviceService<DeviceDetails> deviceService)
            : base(logger, deviceService)
        {
            this.devicePropertyService = devicePropertyService;
        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="searchStatus"></param>
        /// <param name="searchState"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="orderBy"></param>
        /// <param name="modelId"></param>
        /// <param name="labels"></param>
        [HttpGet(Name = "GET Device list")]
        public Task<PaginationResult<DeviceListItem>> SearchItems(
            string searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null,
            string modelId = null,
            [FromQuery] string[] labels = null)
        {
            return GetItems("GET Device list", searchText, searchStatus, searchState, pageSize, pageNumber, orderBy, modelId, labels);
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        [HttpGet("{deviceID}", Name = "GET Device details")]
        public override Task<DeviceDetails> GetItem(string deviceID)
        {
            return base.GetItem(deviceID);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPost(Name = "POST Create device")]
        public override Task<IActionResult> CreateDeviceAsync(DeviceDetails device)
        {
            return base.CreateDeviceAsync(device);
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPut(Name = "PUT Update device")]
        public override Task<IActionResult> UpdateDeviceAsync(DeviceDetails device)
        {
            return base.UpdateDeviceAsync(device);
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        [HttpDelete("{deviceID}", Name = "DELETE Remove device")]
        public override Task<IActionResult> Delete(string deviceID)
        {
            return base.Delete(deviceID);
        }

        /// <summary>
        /// Gets the device credentials.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        [HttpGet("{deviceID}/credentials", Name = "GET Device Credentials")]
        public override Task<ActionResult<DeviceCredentials>> GetCredentials(string deviceID)
        {
            return base.GetCredentials(deviceID);
        }

        /// <summary>
        /// Gets the device credentials.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        [HttpGet("{deviceID}/properties", Name = "GET Device Properties")]
        public async Task<IEnumerable<DevicePropertyValue>> GetProperties(string deviceID)
        {
            return await this.devicePropertyService.GetProperties(deviceID);
        }

        /// <summary>
        /// Gets the device credentials.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        /// <param name="values">The properties values.</param>
        [HttpPost("{deviceID}/properties", Name = "POST Device Properties")]
        public async Task<ActionResult<IEnumerable<DevicePropertyValue>>> SetProperties(string deviceID, IEnumerable<DevicePropertyValue> values)
        {
            await this.devicePropertyService.SetProperties(deviceID, values);

            return Ok();
        }

        [HttpGet("available-labels", Name = "GET Available Labels on Devices")]
        public override Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            return base.GetAvailableLabels();
        }
    }
}
