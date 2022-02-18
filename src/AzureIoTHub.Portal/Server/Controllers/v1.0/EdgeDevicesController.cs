// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/edge/device")]
    [ApiExplorerSettings(GroupName = "IoT Edge")]
    public class EdgeDevicesController : ControllerBase
    {
        /// <summary>
        /// The device  edge devices controller.
        /// </summary>
        private readonly ILogger<EdgeDevicesController> logger;

        /// <summary>
        /// The device registry manager.
        /// </summary>
        private readonly RegistryManager registryManager;

        /// <summary>
        /// The device iconfiguration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// The device idevice service.
        /// </summary>
        private readonly IDeviceService devicesService;

        /// <summary>
        /// The device iconnection string manager.
        /// </summary>
        private readonly IConnectionStringManager connectionStringManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDevicesController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="registryManager">The registry manager.</param>
        /// <param name="connectionStringManager">The connection string manager.</param>
        /// <param name="service">The service.</param>
        public EdgeDevicesController(
            IConfiguration configuration,
            ILogger<EdgeDevicesController> logger,
            RegistryManager registryManager,
            IConnectionStringManager connectionStringManager,
            IDeviceService service)
        {
            this.logger = logger;
            this.registryManager = registryManager;
            this.configuration = configuration;
            this.devicesService = service;
            this.connectionStringManager = connectionStringManager;
        }

        /// <summary>
        /// Gets the IoT Edge device list.
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GET IoT Edge devices")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GatewayListItem>))]
        public async Task<IActionResult> Get()
        {
            // don't contain tags
            IEnumerable<Twin> edgeDevices = await this.devicesService.GetAllEdgeDevice();

            List<GatewayListItem> newGatewayList = new();

            foreach (Twin deviceTwin in edgeDevices)
            {
                var twin = this.devicesService.GetDeviceTwin(deviceTwin.DeviceId).Result;

                if (twin != null)
                {
                    GatewayListItem gateway = new()
                    {
                        DeviceId = deviceTwin.DeviceId,
                        Status = twin.Status?.ToString(),
                        Type = DeviceHelper.RetrieveTagValue(twin, "purpose"),
                        NbDevices = DeviceHelper.RetrieveConnectedDeviceCount(deviceTwin)
                    };

                    newGatewayList.Add(gateway);
                }
            }

            return this.Ok(newGatewayList);
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        [HttpGet("{deviceId}", Name = "GET IoT Edge device")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Gateway))]
        public async Task<IActionResult> Get(string deviceId)
        {
            try
            {
                Twin deviceTwin = await this.devicesService.GetDeviceTwin(deviceId);
                Twin deviceWithModules = await this.devicesService.GetDeviceTwinWithModule(deviceId);

                Gateway gateway = new()
                {
                    DeviceId = deviceTwin.DeviceId,
                    Status = deviceTwin.Status?.ToString(),
                    EndPoint = this.configuration["IoTDPS:ServiceEndpoint"],
                    Scope = deviceTwin.DeviceScope,
                    ConnectionState = deviceTwin.ConnectionState?.ToString(),
                    // We retrieve the values of tags
                    Type = DeviceHelper.RetrieveTagValue(deviceTwin, "purpose"),
                    Environment = DeviceHelper.RetrieveTagValue(deviceTwin, "env"),
                    // We retrieve the number of connected device
                    NbDevices = await this.RetrieveNbConnectedDevice(deviceTwin.DeviceId),
                    // récupération des informations sur le modules de la gateways²
                    NbModules = DeviceHelper.RetrieveNbModuleCount(deviceWithModules, deviceId),
                    RuntimeResponse = DeviceHelper.RetrieveRuntimeResponse(deviceWithModules, deviceId),
                    Modules = DeviceHelper.RetrieveModuleList(deviceWithModules, DeviceHelper.RetrieveNbModuleCount(deviceWithModules, deviceId)),
                    // recup du dernier deployment
                    LastDeployment = await this.RetrieveLastConfiguration(deviceWithModules)
                };

                return this.Ok(gateway);
            }
            catch (DeviceNotFoundException e)
            {
                return this.StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
        }

        /// <summary>
        /// Gets the IoT Edge device symmetric key.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns></returns>
        [HttpGet("{deviceId}/{deviceType}/ConnectionString", Name = "GET Device symmetric key")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSymmetricKey(string deviceId, string deviceType)
        {
            return this.Ok(await this.connectionStringManager.GetSymmetricKey(deviceId, deviceType));
        }

        /// <summary>
        /// Creates the IoT Edge device.
        /// </summary>
        /// <param name="gateway">The IoT Edge device.</param>
        /// <returns></returns>
        [HttpPost(Name = "POST Create IoT Edge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGatewayAsync(Gateway gateway)
        {
            try
            {
                Twin deviceTwin = new(gateway.DeviceId);

                deviceTwin.Tags["env"] = gateway.Environment;
                deviceTwin.Tags["purpose"] = gateway.Type;

                var result = await this.devicesService.CreateDeviceWithTwin(gateway.DeviceId, true, deviceTwin, DeviceStatus.Enabled);
                this.logger.LogInformation($"Created edge device {gateway.DeviceId}");

                return this.Ok(result);
            }
            catch (DeviceAlreadyExistsException e)
            {
                this.logger.LogInformation(e.Message);
                return this.StatusCode(StatusCodes.Status400BadRequest, e.Message);
            }
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="gateway">The IoT Edge device.</param>
        /// <returns></returns>
        [HttpPut(Name = "PUT Update IoT Edge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDeviceAsync(Gateway gateway)
        {
            Device device = await this.devicesService.GetDevice(gateway.DeviceId);

            if (Enum.TryParse(gateway.Status, out DeviceStatus status))
            {
                device.Status = status;
            }

            device = await this.devicesService.UpdateDevice(device);

            Twin deviceTwin = await this.devicesService.GetDeviceTwin(gateway.DeviceId);
            deviceTwin.Tags["env"] = gateway.Environment;
            deviceTwin = await this.devicesService.UpdateDeviceTwin(gateway.DeviceId, deviceTwin);

            this.logger.LogInformation($"iot hub device was updated  {device.Id}");
            return this.Ok(deviceTwin);
        }

        /// <summary>
        /// Deletes the device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        [HttpDelete("{deviceId}", Name = "DELETE Remove IoT Edge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteDeviceAsync(string deviceId)
        {
            await this.devicesService.DeleteDevice(deviceId);
            this.logger.LogInformation($"iot hub device was delete  {deviceId}");

            return this.Ok($"iot hub device was delete  {deviceId}");
        }

        /// <summary>
        /// Executes the module method on the IoT Edge device.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        [HttpPost("{deviceId}/{moduleId}/{methodName}", Name = "POST Execute module command")]
        public async Task<C2Dresult> ExecuteMethode(GatewayModule module, string deviceId, string methodName)
        {
            CloudToDeviceMethod method = new(methodName);
            string payload = string.Empty;

            if (methodName == "RestartModule")
            {
                payload = JsonConvert.SerializeObject(new
                {
                    id = module.ModuleName,
                    schemaVersion = module.Version
                });
            }

            if (methodName == "GetModuleLogs")
            {
                payload = JsonConvert.SerializeObject(new
                {
                    schemaVersion = module.Version,
                    items = new[]
                    {
                            new
                            {
                                id = module.ModuleName,
                                filter = new
                                {
                                    tail = 10
                                }
                            }
                    },
                    encoding = "none",
                    contentType = "json"
                });
            }

            method.SetPayloadJson(payload);

            CloudToDeviceMethodResult result = await this.devicesService.ExecuteC2DMethod(deviceId, method);
            this.logger.LogInformation($"iot hub device : {deviceId} module : {module.ModuleName} execute methode {methodName}.");

            return new C2Dresult()
            {
                Payload = result.GetPayloadAsJson(),
                Status = result.Status
            };
        }

        /// <summary>
        /// Retrieves the connected devices number on the IoT Edge.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        private async Task<int> RetrieveNbConnectedDevice(string deviceId)
        {
            IQuery query = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']", 1);
            Twin deviceWithClient = (await query.GetNextAsTwinAsync()).SingleOrDefault();
            var reportedProperties = JObject.Parse(deviceWithClient.Properties.Reported.ToJson());

            if (reportedProperties.TryGetValue("clients", out JToken clients))
            {
                return clients.Count();
            }

            return 0;
        }

        /// <summary>
        /// Retrieves the last configuration of the IoT Edge.
        /// </summary>
        /// <param name="twin">The twin.</param>
        /// <returns></returns>
        private async Task<ConfigItem> RetrieveLastConfiguration(Twin twin)
        {
            ConfigItem item = new();

            if (twin.Configurations != null && twin.Configurations.Count > 0)
            {
                foreach (var config in twin.Configurations)
                {
                    Configuration confObj = await this.registryManager.GetConfigurationAsync(config.Key);
                    if (item.DateCreation < confObj.CreatedTimeUtc && config.Value.Status == ConfigurationStatus.Applied)
                    {
                        item.Name = config.Key;
                        item.DateCreation = confObj.CreatedTimeUtc;
                        item.Status = ConfigurationStatus.Applied.ToString();
                    }
                }
            }

            return item;
        }
    }
}
