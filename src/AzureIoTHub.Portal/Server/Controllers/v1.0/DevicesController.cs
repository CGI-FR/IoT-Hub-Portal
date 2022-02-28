// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/devices")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class DevicesController : DevicesControllerBase<DeviceListItem, DeviceDetails>
    {
        public DevicesController(
            ILogger<DevicesController> logger,
            IDeviceService devicesService,
            IDeviceTagService deviceTagService,
            IDeviceProvisioningServiceManager deviceProvisioningServiceManager,
            IDeviceTwinMapper<DeviceListItem, DeviceDetails> deviceTwinMapper,
            ITableClientFactory tableClientFactory)
            : base (logger, devicesService, deviceTagService deviceTwinMapper, deviceProvisioningServiceManager, tableClientFactory)
        {

        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GET Device list")]
        public override Task<IEnumerable<DeviceListItem>> Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns></returns>
        [HttpGet("{deviceID}", Name = "GET Device details")]
        public override Task<DeviceDetails> Get(string deviceID)
        {
            return base.Get(deviceID);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        [HttpPost(Name = "POST Create device")]
        public override Task<IActionResult> CreateDeviceAsync(DeviceDetails device)
        {
            return base.CreateDeviceAsync(device);
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        [HttpPut(Name = "PUT Update device")]
        public override Task<IActionResult> UpdateDeviceAsync(DeviceDetails device)
        {
            return base.UpdateDeviceAsync(device);
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns></returns>
        [HttpDelete("{deviceID}", Name = "DELETE Remove device")]
        public override Task<IActionResult> Delete(string deviceID)
        {
            return base.Delete(deviceID);
        }

        /// <summary>
        /// Gets the device credentials.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns></returns>
        [HttpGet("{deviceID}/credentials", Name = "GET Device Credentials")]
        public override Task<ActionResult<EnrollmentCredentials>> GetCredentials(string deviceID)
        {
            return base.GetCredentials(deviceID);
        }
    }
}
