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
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
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

        public GatewaysController(
            IConfiguration configuration,
            ILogger<GatewaysController> logger,
            RegistryManager registryManager,
            ProvisioningServiceClient dps,
            ServiceClient serviceClient)
        {
            this.logger = logger;
            this.registryManager = registryManager;
            this.dps = dps;
            this.serviceClient = serviceClient;
            this.configuration = configuration;
        }

        /// <summary>
        /// Fonction permettant de récupèrer la liste des appareils Edge .
        /// Après avoir éxecuté la query du registryManager on récupère le resultat
        /// sous la forme d'une liste de Twin.
        /// </summary>
        /// <returns>Retourne un IEnumerable de GatewayListItem avec les propriètés que l'on souhaite.</returns>
        [HttpGet]
        public async Task<IEnumerable<GatewayListItem>> Get()
        {
            var query = this.registryManager.CreateQuery("SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' GROUP BY deviceId", 10);
            var query2 = this.registryManager.CreateQuery("SELECT * FROM devices where devices.capabilities.iotEdge = true", 10);
            List<GatewayListItem> gatewayList = new List<GatewayListItem>();

            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                var pageBis = await query2.GetNextAsTwinAsync();
                int index = 0;

                foreach (var twin in page)
                {
                    var result = new GatewayListItem
                    {
                        DeviceId = twin.DeviceId,
                        Status = twin.Status.Value.ToString(),
                        NbDevices = 0,
                        Type = "unknow"
                    };

                    if (twin.Properties.Reported.Contains("clients"))
                    {
                        result.NbDevices = twin.Properties.Reported["clients"].Count;
                    }

                    if (pageBis.ElementAt(index).Tags.Contains("purpose"))
                    {
                        result.Type = pageBis.ElementAt(index).Tags["purpose"];
                    }

                    gatewayList.Add(result);
                    index++;
                }
            }

            return gatewayList;
        }

        [HttpGet("{deviceId}")]
        public async Task<Gateway> Get(string deviceId)
        {
            var deviceTwin = await this.registryManager.GetTwinAsync(deviceId);
            var query = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeAgent' AND deviceId in ['{deviceId}']");

            Gateway gateway = new Gateway();

            gateway.DeviceId = deviceTwin.DeviceId;
            gateway.Status = deviceTwin.Status.Value.ToString();
            gateway.EndPoint = this.configuration["IoTDPS:ServiceEndpoint"];
            gateway.Scope = deviceTwin.DeviceScope;
            gateway.Connection_state = deviceTwin.ConnectionState.Value.ToString();

            // on récupère la symmetric Key
            gateway.SymmetricKey = await this.RetrieveSymmetricKey(gateway.DeviceId);

            // on récupère les valeur contenu dans les tags
            gateway.Type = this.RetrieveTagValue(deviceTwin, "purpose");
            gateway.Environement = this.RetrieveTagValue(deviceTwin, "env");

            // on récupère le nombre d'appareil connecté
            gateway.NbDevices = await this.RetrieveNbConnectedDevice(gateway.DeviceId);

            while (query.HasMoreResults)
            {
                var deviceWithModules = await query.GetNextAsTwinAsync();
                // récupération des informations sur le modules de la gateways
                foreach (var item in deviceWithModules)
                {
                    gateway.NbModule = this.RetrieveNbModuleCount(item, deviceId);

                    gateway.RuntimeResponse = this.RetrieveRuntimeResponse(item, deviceId);

                    if (gateway.NbModule > 0)
                        gateway.Modules = this.RetrieveModuleList(item);

                    // recup du dernier deployment
                    if (item.Configurations != null)
                    {
                        if (item.Configurations.Count > 0)
                        {
                            gateway.LastDeployment = await this.RetrieveLastConfiguration(item);
                        }
                    }
                }
            }

            return gateway;
        }

        [HttpPost]
        public async Task<IActionResult> Add(Gateway gateway)
        {
            try
            {
                var device = new Device(gateway.DeviceId)
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
                var twin = await this.registryManager.GetTwinAsync(device.Id).ConfigureAwait(false);

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
            var device = await this.registryManager.GetDeviceAsync(gateway.DeviceId);
            if (gateway.Status == DeviceStatus.Enabled.ToString())
            {
                device.Status = DeviceStatus.Enabled;
            }
            else
            {
                device.Status = DeviceStatus.Disabled;
            }

            device = await this.registryManager.UpdateDeviceAsync(device);

            var deviceTwin = await this.registryManager.GetTwinAsync(gateway.DeviceId);
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
                CloudToDeviceMethod method = new CloudToDeviceMethod(methodName);
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
        /// This function genefates the symmetricKey of a device
        /// from its Id.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <returns>string.</returns>
        private async Task<string> RetrieveSymmetricKey(string deviceId)
        {
            var attestationMechanism = await this.dps.GetEnrollmentGroupAttestationAsync(this.configuration["IoTDPS:DefaultEnrollmentGroupe"]);

            // then we get the symmetricKey
            SymmetricKeyAttestation symmetricKey = attestationMechanism.GetAttestation() as SymmetricKeyAttestation;
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(symmetricKey.PrimaryKey));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId)));
        }

        /// <summary>
        /// Checks if the specific property exists within the device twin,
        /// Returns the corresponding value if so, else returns a generic value "unknow".
        /// </summary>
        /// <param name="item">the device twin.</param>
        /// <param name="tagName">the tag property.</param>
        /// <returns>string.</returns>
        private string RetrieveTagValue(Twin item, string tagName)
        {
            if (item.Tags.Contains(tagName))
                return item.Tags[tagName];
            else
                return "unknow";
        }

        /// <summary>
        /// this function get and return the number of device connected
        /// to a gateway.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <returns>int.</returns>
        private async Task<int> RetrieveNbConnectedDevice(string deviceId)
        {
            var query = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']");
            var deviceWithClient = await query.GetNextAsTwinAsync();
            var count = 0;
            // récupération des informations sur les clients connecté à la gateway
            foreach (var item in deviceWithClient)
            {
                if (item.Properties.Reported.Contains("clients") && item.DeviceId == deviceId)
                {
                    count = item.Properties.Reported["clients"].Count;
                }
            }

            return count;
        }

        private int RetrieveNbModuleCount(Twin twin, string deviceId)
        {
            if (twin.Properties.Desired.Contains("modules") && twin.DeviceId == deviceId)
                return twin.Properties.Desired["modules"].Count;
            else
                return 0;
        }

        private string RetrieveRuntimeResponse(Twin twin, string deviceId)
        {
            if (twin.Properties.Reported.Contains("systemModules") && twin.DeviceId == deviceId)
            {
                foreach (var element in twin.Properties.Reported["systemModules"])
                {
                    if (element.Key == "edgeAgent")
                    {
                        return element.Value["runtimeStatus"];
                    }
                }
            }

            return string.Empty;
        }

        private List<GatewayModule> RetrieveModuleList(Twin twin)
        {
            List<GatewayModule> list = new List<GatewayModule>();

            if (twin.Properties.Reported.Contains("modules"))
            {
                foreach (var element in twin.Properties.Reported["modules"])
                {
                    var module = new GatewayModule
                    {
                        ModuleName = element.Key
                    };

                    if (element.Value.Contains("status"))
                        module.Status = element.Value["status"];

                    if (element.Value.Contains("version"))
                        module.Version = element.Value["version"];

                    list.Add(module);
                }
            }

            return list;
        }

        private async Task<ConfigItem> RetrieveLastConfiguration(Twin twin)
        {
            ConfigItem item = new ConfigItem();
            foreach (var config in twin.Configurations)
            {
                var confObj = await this.registryManager.GetConfigurationAsync(config.Key);
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
