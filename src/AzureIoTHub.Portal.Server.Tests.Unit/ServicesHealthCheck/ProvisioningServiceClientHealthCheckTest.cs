// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.ServicesHealthCheck
{
    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.ServicesHealthCheck;
    using AzureIoTHub.Portal.Server.Wrappers;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ProvisioningServiceClientHealthCheckTest
    {
        private MockRepository mockRepository;
        private Mock<IProvisioningServiceClient> mockDps;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDps = this.mockRepository.Create<IProvisioningServiceClient>();
        }

        private ProvisioningServiceClientHealthCheck CreateHealthService()
        {
            return new ProvisioningServiceClientHealthCheck(this.mockDps.Object);
        }

        [Test]
        public async Task CheckHealthAsyncStateUnderTestReturnHealthy()
        {
            // Arrange
            var healthService = CreateHealthService();

            var healthCheckContext = new HealthCheckContext();
            var token = new CancellationToken(canceled:false);

            var attestation = new SymmetricKeyAttestation(GenerateKey(), GenerateKey());
            var enrollmentGroup = new EnrollmentGroup("enrollmentId", attestation);

            _ = this.mockDps
                .Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(It.IsAny<EnrollmentGroup>()))
                .ReturnsAsync(enrollmentGroup);

            _ = this.mockDps
                .Setup(c => c.GetEnrollmentGroupAsync(It.IsAny<string>()))
                .ReturnsAsync(enrollmentGroup);

            _ = this.mockDps
                .Setup(c => c.DeleteEnrollmentGroupAsync(It.IsAny<EnrollmentGroup>(), It.IsAny<CancellationToken>()));

            // Act
            var result = await healthService.CheckHealthAsync(healthCheckContext, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }

        private static string GenerateKey()
        {
            const int length = 48;
            var rnd = RandomNumberGenerator.GetBytes(length);

            return Convert.ToBase64String(rnd);
        }

    }
}
