// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Wrappers;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using System.Threading;

    public class DeviceProvisioningServiceManager : IDeviceProvisioningServiceManager
    {
        private readonly IProvisioningServiceClient dps;
        private readonly ConfigHandler config;

        public DeviceProvisioningServiceManager(IProvisioningServiceClient dps, ConfigHandler config)
        {
            this.dps = dps;
            this.config = config;
        }

        public async Task<EnrollmentGroup> CreateEnrollmentGroupAsync(string deviceType)
        {
            var twinState = new TwinState(
                tags: new TwinCollection($"{{ \"deviceType\":\"{deviceType}\" }}"),
                desiredProperties: new TwinCollection());

            return await CreateNewEnrollmentGroup(deviceType, true, twinState);
        }

        public async Task<EnrollmentGroup> CreateEnrollmentGroupFromModelAsync(string modelId, string modelName, TwinCollection desiredProperties)
        {
            var twinState = new TwinState(
                tags: new TwinCollection($"{{ \"modelId\":\"{modelId}\" }}"),
                desiredProperties: new TwinCollection());

            return await CreateNewEnrollmentGroup(modelId, false, twinState);
        }

        private async Task<EnrollmentGroup> CreateNewEnrollmentGroup(string name, bool iotEdge, TwinState initialTwinState)
        {
            var enrollmentGroupName = ComputeEnrollmentGroupName(name);
            EnrollmentGroup enrollmentGroup;

            try
            {
                enrollmentGroup = await this.dps.GetEnrollmentGroupAsync(enrollmentGroupName);
            }
            catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var enrollmentGroupPrimaryKey = GenerateKey();
                var enrollmentGroupSecondaryKey = GenerateKey();

                var attestation = new SymmetricKeyAttestation(enrollmentGroupPrimaryKey, enrollmentGroupSecondaryKey);

                enrollmentGroup = new EnrollmentGroup(enrollmentGroupName, attestation)
                {
                    ProvisioningStatus = ProvisioningStatus.Enabled,
                    Capabilities = new DeviceCapabilities
                    {
                        IotEdge = iotEdge
                    },
                };
            }

            enrollmentGroup.InitialTwinState = initialTwinState;
            enrollmentGroup.Capabilities.IotEdge = iotEdge;

            return await this.dps.CreateOrUpdateEnrollmentGroupAsync(enrollmentGroup);
        }

        public async Task DeleteEnrollmentGroupAsync(EnrollmentGroup enrollmentGroup, CancellationToken cancellationToken = default)
        {
            await this.dps.DeleteEnrollmentGroupAsync(enrollmentGroup, cancellationToken);
        }

        /// <summary>
        /// this function get the attestation mechanism of the DPS.
        /// </summary>
        /// <param name="deviceType"></param>
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
                attestation = await GetAttestation(deviceType);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _ = await CreateEnrollmentGroupAsync(deviceType);
                    attestation = await GetAttestation(deviceType);
                }
                else
                {
                    throw new InvalidOperationException("Failed to get symmetricKey.", e);
                }
            }

            var symmetricKey = DeviceHelper.RetrieveSymmetricKey(deviceId, CheckAttestation(attestation));

            return new EnrollmentCredentials
            {
                SymmetricKey = symmetricKey,
                RegistrationID = deviceId,
                ScopeID = this.config.DPSScopeID,
                ProvisioningEndpoint = $"https://{this.config.DPSEndpoint}"
            };
        }

        private static SymmetricKeyAttestation CheckAttestation(Attestation attestation)
        {
            if (attestation is not SymmetricKeyAttestation symmetricKeyAttestation)
            {
                throw new InvalidOperationException($"Cannot get symmetric key for {attestation.GetType()}.");
            }

            return symmetricKeyAttestation;
        }

        private static string GenerateKey()
        {
            const int length = 48;
            var rnd = RandomNumberGenerator.GetBytes(length);

            return Convert.ToBase64String(rnd);
        }

        private static string ComputeEnrollmentGroupName(string deviceType)
        {
            if (string.IsNullOrEmpty(deviceType))
            {
                return "default";
            }

#pragma warning disable CA1308 // Normalize strings to uppercase
            return deviceType.Trim()
                .ToLowerInvariant()
                .Replace(" ", "-", StringComparison.OrdinalIgnoreCase);
#pragma warning restore CA1308 // Normalize strings to uppercase
        }
    }
}
