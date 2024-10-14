// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Wrappers
{
    public interface IProvisioningServiceClient
    {
        Task<EnrollmentGroup> CreateOrUpdateEnrollmentGroupAsync(EnrollmentGroup enrollmentGroup);

        Task<EnrollmentGroup> GetEnrollmentGroupAsync(string enrollmentGroupId);

        Task<IAttestationMechanism> GetEnrollmentGroupAttestationAsync(string v);

        Task DeleteEnrollmentGroupAsync(EnrollmentGroup enrollmentGroup, CancellationToken cancellationToken);
    }
}
