// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.Device;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class ConcentratorsController : ControllerBase
    {
        private readonly IDeviceService devicesService;
        private readonly IDeviceTwinMapper deviceTwinMapper;

        public ConcentratorsController(
            IDeviceService devicesService,
            IDeviceTwinMapper deviceTwinMapper)
        {
            this.devicesService = devicesService;
            this.deviceTwinMapper = deviceTwinMapper;
        }

        [HttpGet]
        public async Task<IEnumerable<DeviceListItem>> GetAllDeviceConcentrator()
        {
            // Gets all the twins from this devices
            var items = await this.devicesService.GetAllDevice();
            var itemFilter = items.Where(x => x.Tags["deviceType"] == "LoRa Concentrator");

            return itemFilter.Select(this.deviceTwinMapper.CreateDeviceListItem);
        }
    }
}
