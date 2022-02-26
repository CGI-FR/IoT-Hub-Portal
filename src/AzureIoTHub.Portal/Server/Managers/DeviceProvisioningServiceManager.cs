// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
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
            var twinState = new TwinState(
                tags: new TwinCollection($"{{ \"purpose\":\"{deviceType}\" }}"),
                desiredProperties: new TwinCollection());

            return await this.CreateNewEnrollmentGroup(deviceType, true, twinState);
        }

        public async Task<EnrollmentGroup> CreateEnrollmentGroupFormModelAsync(string modelId, string modelName, TwinCollection desiredProperties)
        {
            var twinState = new TwinState(
                tags: new TwinCollection($"{{ \"modelId\":\"{modelId}\" }}"), 
                desiredProperties: new TwinCollection());

            return await this.CreateNewEnrollmentGroup(modelName, false, twinState);
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <returns></returns>
        private async Task<EnrollmentGroup> CreateNewEnrollmentGroup(string name, bool iotEdge, TwinState initialTwinState)
        {
            string enrollmentGroupPrimaryKey = GenerateKey();
            string enrollmentGroupSecondaryKey = GenerateKey();

            SymmetricKeyAttestation attestation = new SymmetricKeyAttestation(enrollmentGroupPrimaryKey, enrollmentGroupSecondaryKey);

            EnrollmentGroup enrollmentGroup = new EnrollmentGroup(ComputeEnrollmentGroupName(name), attestation)
            {
                ProvisioningStatus = ProvisioningStatus.Enabled,
                Capabilities = new DeviceCapabilities
                {
                    IotEdge = iotEdge
                },
                InitialTwinState = initialTwinState
            };

            return await this.dps.CreateOrUpdateEnrollmentGroupAsync(enrollmentGroup);
        }

        /// <summary>
        /// this function get the attestation mechanism of the DPS.
        /// </summary>
        /// <returns>AttestationMechanism.</returns>
        public async Task<Attestation> GetAttestation(string deviceType)
        {
            var attetationMechanism = await this.dps.GetEnrollmentGroupAttestationAsync(ComputeEnrollmentGroupName(deviceType));

            return attetationMechanism.GetAttestation();
        }

        private static string GenerateKey()
        {
            var length = 48;
            var rnd = RandomNumberGenerator.GetBytes(length);

            return Convert.ToBase64String(rnd);
        }

        private static string ComputeEnrollmentGroupName(string deviceType)
        {
            return deviceType.Trim()
                .ToLowerInvariant()
                .Replace(" ", "-");
        }
    }
}
