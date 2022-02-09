// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDevice;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/devices")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    public class LoRaWANDevicesController : DevicesControllerBase<DeviceListItem, LoRaDeviceDetails>
    {
        private readonly ITableClientFactory tableClientFactory;
        private readonly ILoraDeviceMethodManager loraDeviceMethodManager;
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;

        public LoRaWANDevicesController(
            ILogger<LoRaWANDevicesController> logger,
            IDeviceService devicesService,
            IDeviceTwinMapper<DeviceListItem, LoRaDeviceDetails> deviceTwinMapper,
            ITableClientFactory tableClientFactory,
            ILoraDeviceMethodManager loraDeviceMethodManager,
            IDeviceModelCommandMapper deviceModelCommandMapper)
            : base (logger, devicesService, deviceTwinMapper)
        {
            this.tableClientFactory = tableClientFactory;
            this.loraDeviceMethodManager = loraDeviceMethodManager;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
        }

        /// <summary>
        /// Permit to execute cloud to device message.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <param name="commandId">the command who contain the name and the trame.</param>
        /// <returns>a CloudToDeviceMethodResult .</returns>
        [HttpPost("{deviceId}/_command/{commandId}")]
        public async Task<IActionResult> ExecuteCommand(string deviceId, string commandId)
        {
            try
            {
                var commandEntity = this.tableClientFactory
                       .GetDeviceCommands()
                       .Query<TableEntity>(filter: $"RowKey  eq '{commandId}'")
                       .Single();

                var deviceModelCommand = this.deviceModelCommandMapper.GetDeviceModelCommand(commandEntity);

                var result = await this.loraDeviceMethodManager.ExecuteLoRaDeviceMessage(deviceId, deviceModelCommand);

                if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    this.logger.LogError($"{deviceId} - Execute command on device failed \n {result}");

                    throw new FormatException("Incorrect port or invalid DevEui Format.");
                }

                this.logger.LogInformation($"{deviceId} - Execute command: {result}");

                return this.Ok(await result.Content.ReadFromJsonAsync<dynamic>());
            }
            catch (FormatException e)
            {
                this.logger.LogError($"{deviceId} - Execute command on device failed \n {e.Message}");

                return this.BadRequest("Something went wrong when executing the command.");
            }
        }
    }
}
