// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.Device;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    [Route("api/[controller]")]
    [ApiController]
    public class ConcentratorsController : ControllerBase
    {
        private readonly IDeviceService devicesService;
        private readonly IDeviceTwinMapper deviceTwinMapper;
        private readonly ILogger<ConcentratorsController> logger;

        public ConcentratorsController(
            ILogger<ConcentratorsController> logger,
            IDeviceService devicesService,
            IDeviceTwinMapper deviceTwinMapper)
        {
            this.devicesService = devicesService;
            this.deviceTwinMapper = deviceTwinMapper;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<DeviceListItem>> GetAllDeviceConcentrator()
        {
            // Gets all the twins from this devices
            var items = await this.devicesService.GetAllDevice();
            var itemFilter = items.Where(x => x.Tags["deviceType"] == "LoRa Concentrator");

            return itemFilter.Select(this.deviceTwinMapper.CreateDeviceListItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeviceAsync(DeviceDetails device)
        {
            try
            {
                if (!Eui.TryParse(device.DeviceID, out ulong deviceIdConvert))
                {
                    throw new InvalidOperationException("the device id is in the wrong format.");
                }

                var twinProperties = new TwinProperties();

                twinProperties.Desired["NetId"] = 1;
                twinProperties.Desired["JoinEui"] = new List<string> { "0000000000000000", "FFFFFFFFFFFFFFFF" };
                twinProperties.Desired["hwspec"] = "sx1301/1";
                twinProperties.Desired["freq_range"] = new List<string> { "470000000", "510000000" };
                twinProperties.Desired["nocca"] = true;
                twinProperties.Desired["nodc"] = true;
                twinProperties.Desired["nodwell"] = true;

                // Create a new Twin from the form's fields.
                var newTwin = new Twin()
                {
                    DeviceId = device.DeviceID,
                    Properties = twinProperties
                };

                this.deviceTwinMapper.UpdateTwin(newTwin, device);

                DeviceStatus status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin, status);

                return this.Ok(result);
            }
            catch (DeviceAlreadyExistsException e)
            {
                this.logger.LogError($"{device.DeviceID} - Create device failed", e);
                return this.BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                this.logger?.LogError("{a0} - Create device failed \n {a1}", device.DeviceID, e.Message);
                return this.BadRequest(e.Message);
            }
        }
    }
}
