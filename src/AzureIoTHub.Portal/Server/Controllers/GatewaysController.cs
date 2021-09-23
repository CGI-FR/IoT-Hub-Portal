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
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

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
            var query2 = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' AND deviceId in ['{deviceId}']");

            Gateway gateway = new Gateway();

            gateway.DeviceId = deviceTwin.DeviceId;
            gateway.Status = deviceTwin.Status.Value.ToString();
            // var attestationMechanism = await this.dps.GetEnrollmentGroupAttestationAsync("DemoGatewayEnrollmentGroup");
            var attestationMechanism = await this.dps.GetEnrollmentGroupAttestationAsync(this.configuration["IoTDPS:DefaultEnrollmentGroupe"]);
            gateway.EndPoint = this.configuration["IoTDPS:ServiceEndpoint"];
            gateway.Scope = deviceTwin.DeviceScope;

            // on récupère la symmetric Key
            SymmetricKeyAttestation symmetricKey = attestationMechanism.GetAttestation() as SymmetricKeyAttestation;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(symmetricKey.PrimaryKey)))
            {
                gateway.SymmetricKey = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(gateway.DeviceId)));
            }

            if (deviceTwin.Tags.Contains("purpose"))
            {
                gateway.Type = deviceTwin.Tags["purpose"];
            }
            else
            {
                gateway.Type = "unknow";
            }

            if (deviceTwin.Tags.Contains("env"))
            {
                gateway.Environement = deviceTwin.Tags["env"];
            }
            else
            {
                gateway.Environement = "unknow";
            }

            while (query2.HasMoreResults)
            {
                var deviceWithClient = await query2.GetNextAsTwinAsync();
                // récupération des informations sur les clients connecté à la gateway
                foreach (var item in deviceWithClient)
                {
                    if (item.Properties.Reported.Contains("clients") && item.DeviceId == deviceId)
                    {
                        gateway.NbDevices = item.Properties.Reported["clients"].Count;
                    }
                }
            }

            while (query.HasMoreResults)
            {
                var deviceWithModules = await query.GetNextAsTwinAsync();
                // récupération des informations sur le modules de la gateways
                foreach (var item in deviceWithModules)
                {
                    if (item.Properties.Desired.Contains("modules") && item.DeviceId == deviceId)
                    {
                        gateway.NbModule = item.Properties.Desired["modules"].Count;
                    }

                    if (item.Properties.Reported.Contains("systemModules") && item.DeviceId == deviceId)
                    {
                        foreach (var element in item.Properties.Reported["systemModules"])
                        {
                            if (element.Key == "edgeAgent")
                            {
                                gateway.RuntimeResponse = element.Value["runtimeStatus"];
                            }
                        }
                    }

                    if (gateway.NbModule > 0)
                    {
                        if (item.Properties.Reported.Contains("modules"))
                        {
                            foreach (var element in item.Properties.Reported["modules"])
                            {
                                var module = new GatewayModule();
                                module.ModuleName = element.Key;
                                if (element.Value.Contains("status"))
                                {
                                    module.Status = element.Value["status"];
                                }

                                if (element.Value.Contains("version"))
                                {
                                    module.Version = element.Value["version"];
                                }

                                gateway.Modules.Add(module);
                            }
                        }
                    }

                    // recup du dernier deployment
                    if (item.Configurations.Count > 0)
                    {
                        foreach (var config in item.Configurations)
                        {
                            var confObj = await this.registryManager.GetConfigurationAsync(config.Key);
                            if (gateway.LastDeployment.DateCreation < confObj.CreatedTimeUtc && config.Value.Status == ConfigurationStatus.Applied)
                            {
                                gateway.LastDeployment.Name = config.Key;
                                gateway.LastDeployment.DateCreation = confObj.CreatedTimeUtc;
                                gateway.LastDeployment.Status = ConfigurationStatus.Applied.ToString();
                            }
                        }
                    }
                }
            }

            return gateway;
        }

        [HttpPost]
        public async Task<IActionResult> Add(Gateway gateway)
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

        /// <summary>
        /// this function update the environment and the status of a edge device.
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

        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDeviceAsync(string deviceId)
        {
            await this.registryManager.RemoveDeviceAsync(deviceId);
            this.logger.LogInformation($"iot hub device was delete  {deviceId}");

            return this.Ok();
        }

        [HttpGet("{deviceId}/{moduleId}")]
        public async Task<IActionResult> RebootDeviceModule(string moduleId, string deviceId)
        {
            CloudToDeviceMethod method = new CloudToDeviceMethod("reboot");
            method.ResponseTimeout = TimeSpan.FromSeconds(30);

            CloudToDeviceMethodResult result = await this.serviceClient.InvokeDeviceMethodAsync($"{deviceId}", method);
            this.logger.LogInformation($"iot hub device : {deviceId} module : {moduleId} reboot.");
            return this.Ok(result);
        }

        [HttpPost("{gateway}")]
        public async Task<IActionResult> Post(Gateway gateway)
        {
            var device = new Device(gateway.DeviceId);
            var result = await this.registryManager.AddDeviceAsync(device);

            return this.Ok(result);
        }
    }
}
