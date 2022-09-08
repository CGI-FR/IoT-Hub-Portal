// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.Threading.Tasks;
    using Filters;
    using Mappers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using Services;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/devices")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANDevicesController : DevicesControllerBase<DeviceListItem, LoRaDeviceDetails>
    {
        private readonly ILoRaWANDeviceService loRaWANDeviceService;

        public LoRaWANDevicesController(
            ILogger<LoRaWANDevicesController> logger,
            IDeviceService devicesService,
            IDeviceTagService deviceTagService,
            IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails> deviceTwinMapper,
            ILoRaWANDeviceService loRaWANDeviceService)
            : base(logger, devicesService, deviceTagService, deviceTwinMapper)
        {
            this.loRaWANDeviceService = loRaWANDeviceService;
        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        /// <param name="continuationToken"></param>
        /// <param name="searchText"></param>
        /// <param name="searchStatus"></param>
        /// <param name="searchState"></param>
        /// <param name="pageSize"></param>
        [HttpGet(Name = "GET LoRaWAN device list")]
        public Task<PaginationResult<DeviceListItem>> SearchItems(
            string continuationToken = null,
            string searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            int pageSize = 10)
        {
            return GetItems("GET LoRaWAN device list", continuationToken, searchText, searchStatus, searchState, pageSize);
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        [HttpGet("{deviceID}", Name = "GET LoRaWAN device details")]
        public override Task<LoRaDeviceDetails> GetItem(string deviceID)
        {
            return base.GetItem(deviceID);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPost(Name = "POST Create LoRaWAN device")]
        public override Task<IActionResult> CreateDeviceAsync(LoRaDeviceDetails device)
        {
            return base.CreateDeviceAsync(device);
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPut(Name = "PUT Update LoRaWAN device")]
        public override Task<IActionResult> UpdateDeviceAsync(LoRaDeviceDetails device)
        {
            return base.UpdateDeviceAsync(device);
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        [HttpDelete("{deviceID}", Name = "DELETE Remove LoRaWAN device")]
        public override Task<IActionResult> Delete(string deviceID)
        {
            return base.Delete(deviceID);
        }

        /// <summary>
        /// Executes the command on the device..
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="commandId">The command identifier.</param>
        /// <exception cref="System.FormatException">Incorrect port or invalid DevEui Format.</exception>
        [HttpPost("{deviceId}/_command/{commandId}", Name = "POST Execute LoRaWAN command")]
        public async Task<IActionResult> ExecuteCommand(string deviceId, string commandId)
        {
            await this.loRaWANDeviceService.ExecuteLoRaWANCommand(deviceId, commandId);

            return Ok();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public override Task<ActionResult<EnrollmentCredentials>> GetCredentials(string deviceID)
        {
            return Task.FromResult<ActionResult<EnrollmentCredentials>>(NotFound());
        }
    }
}
