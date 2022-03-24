// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.ServicesHealthCheck
{
    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class ProvisioningServiceClientHealthCheck : IHealthCheck
    {
        private readonly ProvisioningServiceClient provisioningServiceClient;

        public ProvisioningServiceClientHealthCheck(ProvisioningServiceClient provisioningServiceClient)
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

                await ExecuteDPSWriteCheckAsync(enrollmentGroup, cancellationToken);
                await ExecuteDPSReadCheck(enrollmentGroup, cancellationToken);

                await this.provisioningServiceClient.DeleteEnrollmentGroupAsync(enrollmentGroup, cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private async Task ExecuteDPSWriteCheckAsync(EnrollmentGroup enrollmentGroup, CancellationToken cancellationToken)
        {
            _ = await this.provisioningServiceClient.CreateOrUpdateEnrollmentGroupAsync(enrollmentGroup, cancellationToken);
        }

        private async Task ExecuteDPSReadCheck(EnrollmentGroup enrollmentGroup, CancellationToken cancellationToken)
        {
            _ = await this.provisioningServiceClient.GetEnrollmentGroupAsync(enrollmentGroup.EnrollmentGroupId, cancellationToken);
        }

        private static string GenerateKey()
        {
            const int length = 48;
            var rnd = RandomNumberGenerator.GetBytes(length);

            return Convert.ToBase64String(rnd);
        }
    }
}
