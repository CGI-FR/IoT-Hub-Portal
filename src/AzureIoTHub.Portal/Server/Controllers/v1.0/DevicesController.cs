// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Application.Abstractions.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Models.v10;
    using Managers;
    using Mappers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Services;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/devices")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class DevicesController : DevicesControllerBase<DeviceListItem, DeviceDetails>
    {
        private readonly IDevicePropertyService devicePropertyService;

        public DevicesController(
            ILogger<DevicesController> logger,
            IDeviceService devicesService,
            IDeviceTagService deviceTagService,
            IDeviceProvisioningServiceManager deviceProvisioningServiceManager,
            IDeviceTwinMapper<DeviceListItem, DeviceDetails> deviceTwinMapper,
            IDevicePropertyService devicePropertyService,
            ITableClientFactory tableClientFactory)
            : base(logger, devicesService, deviceTagService, deviceTwinMapper, deviceProvisioningServiceManager, tableClientFactory)
        {
            this.devicePropertyService = devicePropertyService;
        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        /// <param name="continuationToken"></param>
        /// <param name="searchText"></param>
        /// <param name="searchStatus"></param>
        /// <param name="searchState"></param>
        /// <param name="pageSize"></param>
        [HttpGet(Name = "GET Device list")]
        public Task<PaginationResult<DeviceListItem>> SearchItems(
            string continuationToken = null,
            string searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            int pageSize = 10)
        {
            return base.GetItems("GET Device list", continuationToken, searchText, searchStatus, searchState, pageSize);
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
        public override Task<ActionResult<EnrollmentCredentials>> GetCredentials(string deviceID)
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
    }
}
