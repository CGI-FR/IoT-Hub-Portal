﻿// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Shared.Models.V10;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;

    public interface IDeviceProvisioningServiceManager
    {
        /// <summary>
        /// Gets the device symmetric key attestation for the enrollment group.
        /// </summary>
        /// <param name="deviceType">The device type.</param>
        /// <returns>The corresponding attestation.</returns>
        Task<Attestation> GetAttestation(string deviceType);

        /// <summary>
        /// Creates the Enrollment group fot the specified device type.
        /// </summary>
        /// <param name="deviceType">The device type name.</param>
        /// <returns>An object representing the corresponding enrollment group.</returns>
        Task<EnrollmentGroup> CreateEnrollmentGroupAsync(string deviceType);

        /// <summary>
        /// Create Enrolllment group for the specified device model.
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        /// <param name="modelName">The model name.</param>
        /// <param name="desiredProperties">The desired properties</param>
        /// <returns></returns>
        Task<EnrollmentGroup> CreateEnrollmentGroupFormModelAsync(string modelId, string modelName, TwinCollection desiredProperties);

        /// <summary>
        /// Returns the device enrollment credentials.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceType">The device type.</param>
        /// <returns></returns>
        Task<EnrollmentCredentials> GetEnrollmentCredentialsAsync(string deviceId, string deviceType);
    }
}
