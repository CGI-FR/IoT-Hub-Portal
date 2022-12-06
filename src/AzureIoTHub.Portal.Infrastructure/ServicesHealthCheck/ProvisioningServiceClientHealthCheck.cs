// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck
{
    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Wrappers;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class ProvisioningServiceClientHealthCheck : IHealthCheck
    {
        private readonly IProvisioningServiceClient provisioningServiceClient;

        public ProvisioningServiceClientHealthCheck(IProvisioningServiceClient provisioningServiceClient)
        {
            this.provisioningServiceClient = provisioningServiceClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var enrollemntId = "enrollmentId";
                var attestation = new SymmetricKeyAttestation(GenerateKey(), GenerateKey());
                var enrollmentGroup = new EnrollmentGroup(enrollemntId, attestation);

                await ExecuteDPSWriteCheckAsync(enrollmentGroup);
                await ExecuteDPSReadCheck(enrollmentGroup);

                await this.provisioningServiceClient.DeleteEnrollmentGroupAsync(enrollmentGroup, cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private async Task ExecuteDPSWriteCheckAsync(EnrollmentGroup enrollmentGroup)
        {
            _ = await this.provisioningServiceClient.CreateOrUpdateEnrollmentGroupAsync(enrollmentGroup);
        }

        private async Task ExecuteDPSReadCheck(EnrollmentGroup enrollmentGroup)
        {
            _ = await this.provisioningServiceClient.GetEnrollmentGroupAsync(enrollmentGroup.EnrollmentGroupId);
        }

        private static string GenerateKey()
        {
            const int length = 48;
            var rnd = RandomNumberGenerator.GetBytes(length);

            return Convert.ToBase64String(rnd);
        }
    }
}
