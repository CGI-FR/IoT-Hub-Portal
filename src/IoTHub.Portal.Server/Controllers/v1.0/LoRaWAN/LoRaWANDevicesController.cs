// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v10
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/devices")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANDevicesController : DevicesControllerBase<LoRaDeviceDetails>
    {
        private readonly ILoRaWANCommandService loRaWanCommandService;
        private readonly LoRaGatewayIdList gatewayIdList;
        private readonly IDeviceService<LoRaDeviceDetails> deviceService;

        public LoRaWANDevicesController(
            ILogger<LoRaWANDevicesController> logger,
            ILoRaWANCommandService loRaWanCommandService,
            IDeviceService<LoRaDeviceDetails> deviceService,
            LoRaGatewayIdList gatewayIdList)
            : base(logger, deviceService)
        {
            this.loRaWanCommandService = loRaWanCommandService;
            this.gatewayIdList = gatewayIdList;
            this.deviceService = deviceService;
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
        [HttpGet(Name = "GET LoRaWAN device list")]
        [Authorize("device:read")]
        public Task<PaginationResult<DeviceListItem>> SearchItems(
            string searchText = null,
            bool? searchStatus = null,
            bool? searchState = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null,
            string modelId = null)
        {
            return GetItems("GET LoRaWAN device list", searchText, searchStatus, searchState, pageSize, pageNumber, orderBy, modelId);
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        [HttpGet("{deviceId}", Name = "GET LoRaWAN device details")]
        [Authorize("device:read")]
        public override Task<LoRaDeviceDetails> GetItem(string deviceId)
        {
            return base.GetItem(deviceId);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPost(Name = "POST Create LoRaWAN device")]
        [Authorize("device:write")]
        public override Task<IActionResult> CreateDeviceAsync(LoRaDeviceDetails device)
        {
            return base.CreateDeviceAsync(device);
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPut(Name = "PUT Update LoRaWAN device")]
        [Authorize("device:write")]
        public override Task<IActionResult> UpdateDeviceAsync(LoRaDeviceDetails device)
        {
            return base.UpdateDeviceAsync(device);
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        [HttpDelete("{deviceId}", Name = "DELETE Remove LoRaWAN device")]
        [Authorize("device:write")]
        public override Task<IActionResult> Delete(string deviceId)
        {
            return base.Delete(deviceId);
        }

        /// <summary>
        /// Executes the command on the device..
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="commandId">The command identifier.</param>
        /// <exception cref="System.FormatException">Incorrect port or invalid DevEui Format.</exception>
        [HttpPost("{deviceId}/_command/{commandId}", Name = "POST Execute LoRaWAN command")]
        [Authorize("device:execute")]
        public async Task<IActionResult> ExecuteCommand(string deviceId, string commandId)
        {
            await this.loRaWanCommandService.ExecuteLoRaWANCommand(deviceId, commandId);

            return Ok();
        }

        [Authorize("device:read")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public override Task<ActionResult<DeviceCredentials>> GetCredentials(string deviceID)
        {
            return Task.FromResult<ActionResult<DeviceCredentials>>(NotFound());
        }

        [HttpGet("gateways", Name = "Get Gateways")]
        [Authorize("device:read")]
        public ActionResult<LoRaGatewayIdList> GetGateways()
        {
            return Ok(this.gatewayIdList);
        }

        [HttpGet("{deviceId}/telemetry")]
        [Authorize("device:read")]
        public Task<IEnumerable<LoRaDeviceTelemetryDto>> GetDeviceTelemetry(string deviceId)
        {
            return this.deviceService.GetDeviceTelemetry(deviceId);
        }

        [HttpGet("available-labels", Name = "GET Available Labels on LoRaWAN Devices")]
        [Authorize("device:read")]
        public override Task<IEnumerable<LabelDto>> GetAvailableLabels()
        {
            return base.GetAvailableLabels();
        }
    }
}
