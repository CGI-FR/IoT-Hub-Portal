// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/models")]
    [ApiExplorerSettings(GroupName = "Device Models")]
    public class DeviceModelsController : DeviceModelsControllerBase<DeviceModel, DeviceModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModelsController"/> class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="deviceModelImageManager">The device model image manager.</param>
        /// <param name="deviceModelMapper">The device model mapper.</param>
        /// <param name="devicesService">The devices service.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
        public DeviceModelsController(ILogger<DeviceModelsControllerBase<DeviceModel, DeviceModel>> log, 
            IDeviceModelImageManager deviceModelImageManager, 
            IDeviceModelMapper<DeviceModel, DeviceModel> deviceModelMapper, 
            IDeviceService devicesService, 
            ITableClientFactory tableClientFactory) 
            : base(log, deviceModelImageManager, deviceModelMapper, devicesService, tableClientFactory, $"")
        {
        }
    }
}