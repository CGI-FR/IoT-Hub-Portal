// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    public class GatewaysController : ControllerBase
    {
        private readonly ILogger<GatewaysController> logger;

        private readonly RegistryManager registryManager;
        private readonly ProvisioningServiceClient dps;
        private readonly ServiceClient serviceClient;
        private readonly IConfiguration configuration;
        private readonly DevicesServices devicesService;

        public GatewaysController(
            IConfiguration configuration,
            ILogger<GatewaysController> logger,
            RegistryManager registryManager,
            ProvisioningServiceClient dps,
            DevicesServices service,
            ServiceClient serviceClient)
        {
            this.logger = logger;
            this.registryManager = registryManager;
            this.dps = dps;
            this.serviceClient = serviceClient;
            this.configuration = configuration;
            this.devicesService = service;
        }

        /// <summary>
        /// Fonction permettant de récupèrer la liste des appareils Edge .
        /// Après avoir éxecuté la query du registryManager on récupère le resultat
        /// sous la forme d'une liste de Twin.
        /// </summary>
        /// <returns>List of GatewayListItem.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GatewayListItem>))]
        public IActionResult Get()
        {
            try
            {
                // ne contient pas les proprieter
                IEnumerable<Twin> devicesWithoutProperties = this.devicesService.GetAllEdgeDeviceWithTags();
                // ne contien pas les appareil connecter
                IEnumerable<Twin> edgeDevices = this.devicesService.GetAllEdgeDevice();

                List<GatewayListItem> newGatewayList = new ();
                int index = 0;

                foreach (Twin deviceTwin in edgeDevices)
                {
                    GatewayListItem gateway = new ()
                    {
                        DeviceId = deviceTwin.DeviceId,
                        Status = deviceTwin.Status.Value.ToString(),
                        Type = Helpers.RetrieveTagValue(devicesWithoutProperties.ElementAt(index), "purpose"),
                        NbDevices = 0
                    };
                    if (deviceTwin.Properties.Reported.Contains("clients"))
                    {
                        gateway.NbDevices = deviceTwin.Properties.Reported["clients"].Count;
                    }

                    newGatewayList.Add(gateway);
                    index++;
                }

                return this.Ok(newGatewayList);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, e.Message);
            }
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
                Twin deviceTwin = this.devicesService.GetDeviceTwin(deviceId);
                Twin deviceWithModules = this.devicesService.GetDeviceWithModule(deviceId);

                Gateway gateway = new ()
                {
                    DeviceId = deviceTwin.DeviceId,
                    Status = deviceTwin.Status.Value.ToString(),
                    EndPoint = this.configuration["IoTDPS:ServiceEndpoint"],
                    Scope = deviceTwin.DeviceScope,
                    Connection_state = deviceTwin.ConnectionState.Value.ToString(),
                    // we retrieve the symmetric Key
                    SymmetricKey = Helpers.RetrieveSymmetricKey(deviceTwin.DeviceId, this.devicesService.GetDpsAttestionMechanism()),
                    // We retrieve the values of tags
                    Type = Helpers.RetrieveTagValue(deviceTwin, "purpose"),
                    Environement = Helpers.RetrieveTagValue(deviceTwin, "env"),
                    // We retrieve the number of connected device
                    NbDevices = await this.RetrieveNbConnectedDevice(deviceTwin.DeviceId),
                    // récupération des informations sur le modules de la gateways
                    NbModule = Helpers.RetrieveNbModuleCount(deviceWithModules, deviceId),
                    RuntimeResponse = Helpers.RetrieveRuntimeResponse(deviceWithModules, deviceId),
                    Modules = Helpers.RetrieveModuleList(deviceWithModules, Helpers.RetrieveNbModuleCount(deviceWithModules, deviceId))
                };

                // recup du dernier deployment
                if (deviceWithModules.Configurations != null)
                {
                    if (deviceWithModules.Configurations.Count > 0)
                    {
                        gateway.LastDeployment = await this.RetrieveLastConfiguration(deviceWithModules);
                    }
                }

                return this.Ok(gateway);
            }
            catch (DeviceNotFoundException e)
            {
                return this.StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(Gateway gateway)
        {
            try
            {
                Device device = new (gateway.DeviceId)
                {
                    Capabilities = new DeviceCapabilities
                    {
                        IotEdge = true,
                    }
                };
                // Création de l'appareil
                await this.registryManager.AddDeviceAsync(device).ConfigureAwait(false);

                this.logger.LogInformation($"Created edge device {device.Id}");
                // Configuration du Twin
                Twin twin = await this.registryManager.GetTwinAsync(device.Id).ConfigureAwait(false);

                this.logger.LogInformation($"\tTwin is {twin.ToJson()}");

                twin.Tags["env"] = gateway.Environement;
                twin.Tags["purpose"] = gateway.Type;

                await this.registryManager.UpdateTwinAsync(device.Id, twin, twin.ETag);
                this.logger.LogInformation($"\tUpdated twin to {twin.ToJson()}");

                return this.Ok(device);
            }
            catch (DeviceAlreadyExistsException e)
            {
                this.logger.LogInformation(e.Message);
                return this.Ok("Device already exist.");
            }
            catch (Exception)
            {
                throw;
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
            Device device = await this.registryManager.GetDeviceAsync(gateway.DeviceId);
            if (gateway.Status == DeviceStatus.Enabled.ToString())
            {
                device.Status = DeviceStatus.Enabled;
            }
            else
            {
                device.Status = DeviceStatus.Disabled;
            }

            device = await this.registryManager.UpdateDeviceAsync(device);

            Twin deviceTwin = await this.registryManager.GetTwinAsync(gateway.DeviceId);
            deviceTwin.Tags["env"] = gateway.Environement;
            deviceTwin = await this.registryManager.UpdateTwinAsync(device.Id, deviceTwin, deviceTwin.ETag);

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
            try
            {
                await this.registryManager.RemoveDeviceAsync(deviceId);
                this.logger.LogInformation($"iot hub device was delete  {deviceId}");

                return this.Ok($"iot hub device was delete  {deviceId}");
            }
            catch (Exception e)
            {
                return this.Problem("Somethink went wrong.", e.Message, 400);
            }
        }

        [HttpPost("{deviceId}/{moduleId}/{methodName}")]
        public async Task<C2Dresult> ExecuteMethode(GatewayModule module, string deviceId, string methodName)
        {
            try
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

                CloudToDeviceMethodResult result = await this.serviceClient.InvokeDeviceMethodAsync(deviceId, "$edgeAgent", method);
                this.logger.LogInformation($"iot hub device : {deviceId} module : {module.ModuleName} execute methode {methodName}.");

                return new C2Dresult()
                {
                    Payload = result.GetPayloadAsJson(),
                    Status = result.Status
                };
            }
            catch (Exception e)
            {
                return new C2Dresult()
                {
                    Payload = e.Message
                };
            }
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
            IEnumerable<Twin> deviceWithClient = await query.GetNextAsTwinAsync();
            int count = 0;
            // récupération des informations sur les clients connecté à la gateway
            foreach (Twin item in deviceWithClient)
            {
                if (item.Properties.Reported.Contains("clients") && item.DeviceId == deviceId)
                {
                    count = item.Properties.Reported["clients"].Count;
                    break;
                }
            }

            return count;
        }

        /// <summary>
        /// This function found the last conguration deployed on the device.
        /// </summary>
        /// <param name="twin">the twin of the device we want.</param>
        /// <returns>ConfigItem.</returns>
        private async Task<ConfigItem> RetrieveLastConfiguration(Twin twin)
        {
            ConfigItem item = new ();
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

            return item;
        }
    }
}
