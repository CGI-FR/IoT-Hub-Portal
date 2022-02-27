// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Wrappers
{
    using System.Net.Http;
    using System.Threading.Tasks;
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
    }
}
