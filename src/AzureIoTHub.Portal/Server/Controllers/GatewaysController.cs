// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [ApiController]
    [Route("api/[controller]")]
    public class GatewaysController : ControllerBase
    {
        private readonly ILogger<GatewaysController> logger;

        private readonly RegistryManager registryManager;
        private readonly IConfiguration configuration;
        private readonly IDeviceService devicesService;
        private readonly IConnectionStringManager connectionStringManager;

        public GatewaysController(
            IConfiguration configuration,
            ILogger<GatewaysController> logger,
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
        /// Fonction permettant de récupèrer la liste des appareils Edge .
        /// Après avoir éxecuté la query du registryManager on récupère le resultat
        /// sous la forme d'une liste de Twin.
        /// </summary>
        /// <returns>List of GatewayListItem.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GatewayListItem>))]
        public async Task<IActionResult> Get()
        {
            // don't contain properties
            IEnumerable<Twin> devicesWithoutProperties = await this.devicesService.GetAllEdgeDeviceWithTags();
            // don't contain connected device
            IEnumerable<Twin> edgeDevices = await this.devicesService.GetAllEdgeDevice();

            List<GatewayListItem> newGatewayList = new ();
            int index = 0;

            foreach (Twin deviceTwin in edgeDevices)
            {
                GatewayListItem gateway = new ()
                {
                    DeviceId = deviceTwin.DeviceId,
                    Status = deviceTwin.Status?.ToString(),
                    Type = DeviceHelper.RetrieveTagValue(devicesWithoutProperties.ElementAt(index), "purpose"),
                    NbDevices = DeviceHelper.RetrieveConnectedDeviceCount(deviceTwin)
                };

                newGatewayList.Add(gateway);
                index++;
            }

            return this.Ok(newGatewayList);
        }

        /// <summary>
        /// This function return all the information we want of
        /// a device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <returns>Gateway.</returns>
        [HttpGet("{deviceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Gateway))]
        public async Task<IActionResult> Get(string deviceId)
        {
            try
            {
                Twin deviceTwin = await this.devicesService.GetDeviceTwin(deviceId);
                Twin deviceWithModules = await this.devicesService.GetDeviceTwinWithModule(deviceId);

                Gateway gateway = new ()
                {
                    DeviceId = deviceTwin.DeviceId,
                    Status = deviceTwin.Status?.ToString(),
                    EndPoint = this.configuration["IoTDPS:ServiceEndpoint"],
                    Scope = deviceTwin.DeviceScope,
                    Connection_state = deviceTwin.ConnectionState?.ToString(),
                    // we retrieve the symmetric Key
                    // SymmetricKey = DeviceHelper.RetrieveSymmetricKey(deviceTwin.DeviceId, this.devicesService.GetDpsAttestionMechanism().Result),
                    // We retrieve the values of tags
                    Type = DeviceHelper.RetrieveTagValue(deviceTwin, "purpose"),
                    Environment = DeviceHelper.RetrieveTagValue(deviceTwin, "env"),
                    // We retrieve the number of connected device
                    NbDevices = await this.RetrieveNbConnectedDevice(deviceTwin.DeviceId),
                    // récupération des informations sur le modules de la gateways²
                    NbModule = DeviceHelper.RetrieveNbModuleCount(deviceWithModules, deviceId),
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

        [HttpGet("{deviceId}/{deviceType}/ConnectionString")]
        public async Task<IActionResult> GetSymmetricKey(string deviceId, string deviceType = "unknown")
        {
            return this.Ok(await this.connectionStringManager.GetSymmetricKey(deviceId, deviceType));
        }

        /// <summary>
        /// this function create a device with the twin information.
        /// </summary>
        /// <param name="gateway">the gateway object.</param>
        /// <returns>Bulk registry operation result.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateGatewayAsync(Gateway gateway)
        {
            try
            {
                Twin deviceTwin = new (gateway.DeviceId);

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
        /// This function update the properties of a device.
        /// </summary>
        /// <param name="gateway">a gateways object.</param>
        /// <returns>the twin of the device.</returns>
        [HttpPut("{gateway}")]
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
        /// this function delete a device.
        /// </summary>
        /// <param name="deviceId">the device id to delete.</param>
        /// <returns>message.</returns>
        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDeviceAsync(string deviceId)
        {
            await this.devicesService.DeleteDevice(deviceId);
            this.logger.LogInformation($"iot hub device was delete  {deviceId}");

            return this.Ok($"iot hub device was delete  {deviceId}");
        }

        [HttpPost("{deviceId}/{moduleId}/{methodName}")]
        public async Task<C2Dresult> ExecuteMethode(GatewayModule module, string deviceId, string methodName)
        {
            CloudToDeviceMethod method = new (methodName);
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
        /// this function get and return the number of device connected
        /// to a gateway.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <returns>int.</returns>
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
        /// This function found the last conguration deployed on the device.
        /// </summary>
        /// <param name="twin">the twin of the device we want.</param>
        /// <returns>ConfigItem.</returns>
        private async Task<ConfigItem> RetrieveLastConfiguration(Twin twin)
        {
            ConfigItem item = new ();

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
