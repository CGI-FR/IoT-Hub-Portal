// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Shared.Models.V10;
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
                tags: new TwinCollection($"{{ \"deviceType\":\"{deviceType}\" }}"),
                desiredProperties: new TwinCollection());

            return await this.CreateNewEnrollmentGroup(deviceType, true, twinState);
        }

        public async Task<EnrollmentGroup> CreateEnrollmentGroupFormModelAsync(string modelId, string modelName, TwinCollection desiredProperties)
        {
            var twinState = new TwinState(
                tags: new TwinCollection($"{{ \"modelId\":\"{modelId}\", \"deviceType\": \"{modelName}\" }}"),
                desiredProperties: new TwinCollection());

            return await this.CreateNewEnrollmentGroup(modelName, false, twinState);
        }

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

        public async Task<EnrollmentCredentials> GetEnrollmentCredentialsAsync(string deviceId, string deviceType)
        {
            Attestation attestation;

            try
            {
                attestation = await this.GetAttestation(deviceType);
            }
            catch (ProvisioningServiceClientHttpException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await this.CreateEnrollmentGroupAsync(deviceType);
                    attestation = await this.GetAttestation(deviceType);
                }
                else
                {
                    throw new InvalidOperationException("Failed to get symmetricKey.", e);
                }
            }

            var symmetricKey = DeviceHelper.RetrieveSymmetricKey(deviceId, this.CheckAttestation(attestation));

            return new EnrollmentCredentials
            {
                SymmetricKey = symmetricKey,
                RegistrationID = deviceId,
                ProvisioningEndpoint = this.config.DPSEndpoint,
                ScopeID = this.config.DPSIDScope
            };
        }
        private SymmetricKeyAttestation CheckAttestation(Attestation attestation)
        {
            var symmetricKeyAttestation = attestation as SymmetricKeyAttestation;

            if (symmetricKeyAttestation == null)
            {
                throw new InvalidOperationException($"Cannot get symmetric key for {attestation.GetType()}.");
            }

            return symmetricKeyAttestation;
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
