// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Wrappers
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Wrappers;
    using Microsoft.Azure.Devices.Provisioning.Service;

    public class ProvisioningServiceClientWrapper : IProvisioningServiceClient
    {
        private readonly ProvisioningServiceClient provisioningServiceClient;

        public ProvisioningServiceClientWrapper(ProvisioningServiceClient client)
        {
            this.provisioningServiceClient = client;
        }

        public Task<EnrollmentGroup> CreateOrUpdateEnrollmentGroupAsync(EnrollmentGroup enrollmentGroup)
        {
            return this.provisioningServiceClient.CreateOrUpdateEnrollmentGroupAsync(enrollmentGroup);
        }

        public async Task<EnrollmentGroup> GetEnrollmentGroupAsync(string enrollmentGroupId)
        {
            try
            {
                return await this.provisioningServiceClient.GetEnrollmentGroupAsync(enrollmentGroupId);
            }
            catch (ProvisioningServiceClientHttpException provExc)
            {
                throw new HttpRequestException(provExc.ErrorMessage, provExc, provExc.StatusCode);
            }
        }

        public async Task<IAttestationMechanism> GetEnrollmentGroupAttestationAsync(string v)
        {
            try
            {
                return new AttestationMechanismWrapper(await this.provisioningServiceClient.GetEnrollmentGroupAttestationAsync(v));
            }
            catch (ProvisioningServiceClientHttpException e)
            {
                throw new HttpRequestException(e.ErrorMessage, e, e.StatusCode);
            }
        }

        public async Task DeleteEnrollmentGroupAsync(EnrollmentGroup enrollmentGroup, CancellationToken cancellationToken = default)
        {
            await this.provisioningServiceClient.DeleteEnrollmentGroupAsync(enrollmentGroup, cancellationToken);
        }
    }
}
