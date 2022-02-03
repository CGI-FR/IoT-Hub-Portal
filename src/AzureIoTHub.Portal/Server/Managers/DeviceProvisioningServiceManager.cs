// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;

    public class DeviceProvisioningServiceManager : IDeviceProvisioningServiceManager
    {
        private readonly IConfiguration configuration;
        private readonly ProvisioningServiceClient dps;

        public DeviceProvisioningServiceManager(IConfiguration configuration, ProvisioningServiceClient dps)
        {
            this.configuration = configuration;
            this.dps = dps;
        }

        public async Task<EnrollmentGroup> CreateEnrollmentGroupAsync()
        {
            string enrollmentGroupId = this.configuration["IoTDPS:DefaultEnrollmentGroup"];
            TwinCollection tags = new TwinCollection("{ }");
            TwinCollection desiredProperties = new TwinCollection("{ }");

            string enrollmentGroupPrimaryKey = Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty);
            string enrollmentGroupSecondaryKey = Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty);

            SymmetricKeyAttestation attestation = new SymmetricKeyAttestation(enrollmentGroupPrimaryKey, enrollmentGroupSecondaryKey);

            EnrollmentGroup enrollmentGroup = new EnrollmentGroup(enrollmentGroupId, attestation)
            {
                ProvisioningStatus = ProvisioningStatus.Enabled,
                Capabilities = new DeviceCapabilities
                {
                    IotEdge = false
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
        public async Task<AttestationMechanism> GetAttestationMechanism()
        {
            return await this.dps.GetEnrollmentGroupAttestationAsync(this.configuration["IoTDPS:DefaultEnrollmentGroup"]);
        }
    }
}
