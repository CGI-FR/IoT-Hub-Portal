// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/devices")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class DevicesController : DevicesControllerBase<DeviceListItem, DeviceDetails>
    {
        public DevicesController(
            ILogger<DevicesController> logger,
            IDeviceService devicesService,
            IDeviceTwinMapper<DeviceListItem, DeviceDetails> deviceTwinMapper)
            : base (logger, devicesService, deviceTwinMapper)
        {

        }
    }
}
