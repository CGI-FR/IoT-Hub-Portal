// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using static AzureIoTHub.Portal.Server.Startup;

    public class DeviceProvisioningServiceManager : IDeviceProvisioningServiceManager
    {
        private readonly ProvisioningServiceClient dps;
        private readonly ConfigHandler config;

        public DeviceProvisioningServiceManager(ProvisioningServiceClient dps, ConfigHandler config)
        {
            this.dps = dps;
            this.config = config;
        }

        public async Task<EnrollmentGroup> CreateEnrollmentGroupAsync(string deviceType)
        {
            string enrollmentGroupId;
            TwinCollection tags;
            TwinCollection desiredProperties;

            if (deviceType == "LoRa Device")
            {
                enrollmentGroupId = this.config.DPSLoRaEnrollmentGroup;
                tags = new TwinCollection("{ \"purpose\":\"" + "LoRaNetworkServer" + "\" }");
                desiredProperties = new TwinCollection("{ }");
            }
            else
            {
                enrollmentGroupId = this.config.DPSDefaultEnrollmentGroup;
                tags = new TwinCollection("{ \"purpose\":\"" + "unknown" + "\" }");
                desiredProperties = new TwinCollection("{ }");
            }

            string enrollmentGroupPrimaryKey = GenerateKey();
            string enrollmentGroupSecondaryKey = GenerateKey();

            SymmetricKeyAttestation attestation = new SymmetricKeyAttestation(enrollmentGroupPrimaryKey, enrollmentGroupSecondaryKey);

            EnrollmentGroup enrollmentGroup = new EnrollmentGroup(enrollmentGroupId, attestation)
            {
                ProvisioningStatus = ProvisioningStatus.Enabled,
                Capabilities = new DeviceCapabilities
                {
                    IotEdge = true
                },
                InitialTwinState = new TwinState(tags, desiredProperties)
            };

            var enrollmentResult = await this.dps.CreateOrUpdateEnrollmentGroupAsync(enrollmentGroup).ConfigureAwait(false);

            return enrollmentResult;
        }

        /// <summary>
        /// this function get the attestation mechanism of the DPS.
        /// </summary>
        /// <returns>AttestationMechanism.</returns>
        public async Task<AttestationMechanism> GetAttestationMechanism(string deviceType)
        {
            if (deviceType == "LoRa Device")
            {
                return await this.dps.GetEnrollmentGroupAttestationAsync(this.config.DPSLoRaEnrollmentGroup);
            }
            else
            {
                return await this.dps.GetEnrollmentGroupAttestationAsync(this.config.DPSDefaultEnrollmentGroup);
            }
        }

        private static string GenerateKey()
        {
            var length = 48;
            var rnd = RandomNumberGenerator.GetBytes(length);

            return Convert.ToBase64String(rnd);
        }
    }
}
