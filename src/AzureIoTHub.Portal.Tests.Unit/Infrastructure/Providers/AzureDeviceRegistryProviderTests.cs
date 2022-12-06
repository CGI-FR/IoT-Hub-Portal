// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Providers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Wrappers;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Infrastructure.Providers;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AzureDeviceRegistryProviderTests
    {
        private MockRepository mockRepository;

        private Mock<IProvisioningServiceClient> mockProvisioningServiceClient;
        private Mock<DevelopmentConfigHandler> mockConfigHandler;
        private Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.mockProvisioningServiceClient = this.mockRepository.Create<IProvisioningServiceClient>();
            this.mockConfigHandler = this.mockRepository.Create<DevelopmentConfigHandler>(this.mockConfiguration.Object);
        }

        private AzureDeviceRegistryProvider CreateManager()
        {
            return new AzureDeviceRegistryProvider(
                this.mockProvisioningServiceClient.Object,
                this.mockConfigHandler.Object);
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task CreateEnrollmentGroupAsyncShouldCreateANewEnrollmentGroup(string deviceType, string enrollmentGroupName)
        {
            // Arrange
            var manager = CreateManager();
            EnrollmentGroup enrollmentGroup = null;

            _ = this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .Throws(new HttpRequestException(null, null, HttpStatusCode.NotFound));

            _ = this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
                 It.Is<EnrollmentGroup>(x => x.EnrollmentGroupId == enrollmentGroupName && x.Attestation is SymmetricKeyAttestation)))
                .ReturnsAsync((EnrollmentGroup e) =>
                {
                    enrollmentGroup = e;
                    return e;
                });

            // Act
            var result = await manager.CreateEnrollmentGroupAsync(deviceType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(enrollmentGroup, result);
            Assert.IsTrue(enrollmentGroup.Capabilities.IotEdge);
            Assert.IsNotNull(enrollmentGroup.InitialTwinState);
            Assert.IsTrue(enrollmentGroup.InitialTwinState.Tags.Contains("deviceType"));
            Assert.AreEqual(deviceType, enrollmentGroup.InitialTwinState.Tags["deviceType"].ToString());

            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task WhenEnrollmentGroupAlreadyExistCreateEnrollmentGroupAsyncShouldUpdateEnrollmentGroup(string deviceType, string enrollmentGroupName)
        {
            // Arrange
            var manager = CreateManager();
            EnrollmentGroup enrollmentGroup = null;
            var attestation = new SymmetricKeyAttestation("aaa", "bbb");

            _ = this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .ReturnsAsync(new EnrollmentGroup(enrollmentGroupName, attestation)
                {
                    Capabilities = new DeviceCapabilities
                    {
                        IotEdge = false
                    }
                });

            _ = this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
                It.Is<EnrollmentGroup>(x => x.EnrollmentGroupId == enrollmentGroupName && x.Attestation is SymmetricKeyAttestation)))
                .ReturnsAsync((EnrollmentGroup e) =>
                {
                    enrollmentGroup = e;
                    return e;
                });

            // Act
            var result = await manager.CreateEnrollmentGroupAsync(deviceType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(enrollmentGroup, result);
            Assert.IsTrue(enrollmentGroup.Capabilities.IotEdge);
            Assert.IsNotNull(enrollmentGroup.InitialTwinState);
            Assert.IsTrue(enrollmentGroup.InitialTwinState.Tags.Contains("deviceType"));
            Assert.AreEqual(deviceType, enrollmentGroup.InitialTwinState.Tags["deviceType"].ToString());
            Assert.IsAssignableFrom<SymmetricKeyAttestation>(result.Attestation);
            Assert.AreEqual(attestation, result.Attestation);

            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task CreateEnrollmentGroupFromModelAsyncShouldCreateANewEnrollmentGroup(string modelId, string enrollmentGroupName)
        {
            // Arrange
            var manager = CreateManager();
            const string modelName = "bbb";
            var desiredProperties = new TwinCollection();
            EnrollmentGroup enrollmentGroup = null;

            _ = this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .Throws(new HttpRequestException(null, null, HttpStatusCode.NotFound));

            _ = this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
                It.Is<EnrollmentGroup>(x =>
                x.EnrollmentGroupId == enrollmentGroupName &&
                x.Attestation is SymmetricKeyAttestation)))
                .ReturnsAsync((EnrollmentGroup e) =>
                {
                    enrollmentGroup = e;
                    return e;
                });

            // Act
            var result = await manager.CreateEnrollmentGroupFromModelAsync(
                modelId,
                modelName,
                desiredProperties);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(enrollmentGroup, result);
            Assert.IsFalse(enrollmentGroup.Capabilities.IotEdge);
            Assert.IsNotNull(enrollmentGroup.InitialTwinState);
            Assert.IsTrue(enrollmentGroup.InitialTwinState.Tags.Contains("modelId"));
            Assert.AreEqual(modelId, enrollmentGroup.InitialTwinState.Tags["modelId"].ToString());
            Assert.AreEqual(desiredProperties, enrollmentGroup.InitialTwinState.DesiredProperties);

            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task WhenEnrollmentGroupExistCreateEnrollmentGroupFromModelAsyncShouldUpdate(string modelId, string enrollmentGroupName)
        {
            // Arrange
            var manager = CreateManager();
            const string modelName = "bbb";
            var desiredProperties = new TwinCollection();
            EnrollmentGroup enrollmentGroup = null;

            var attestation = new SymmetricKeyAttestation("aaa", "bbb");

            _ = this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .ReturnsAsync(new EnrollmentGroup(enrollmentGroupName, attestation)
                {
                    Capabilities = new DeviceCapabilities
                    {
                        IotEdge = true
                    }
                });

            _ = this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
                It.Is<EnrollmentGroup>(x =>
                x.EnrollmentGroupId == enrollmentGroupName &&
                x.Attestation is SymmetricKeyAttestation)))
                .ReturnsAsync((EnrollmentGroup e) =>
                {
                    enrollmentGroup = e;
                    return e;
                });

            // Act
            var result = await manager.CreateEnrollmentGroupFromModelAsync(
                modelId,
                modelName,
                desiredProperties);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(enrollmentGroup, result);
            Assert.IsFalse(enrollmentGroup.Capabilities.IotEdge);
            Assert.IsNotNull(enrollmentGroup.InitialTwinState);
            Assert.IsTrue(enrollmentGroup.InitialTwinState.Tags.Contains("modelId"));
            Assert.AreEqual(modelId, enrollmentGroup.InitialTwinState.Tags["modelId"].ToString());
            Assert.AreEqual(desiredProperties, enrollmentGroup.InitialTwinState.DesiredProperties);
            Assert.IsAssignableFrom<SymmetricKeyAttestation>(result.Attestation);
            Assert.AreEqual(attestation, result.Attestation);

            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task GetAttestationShouldReturnDPSAttestation(string modelName, string enrollmentGroupName)
        {
            // Arrange
            var manager = CreateManager();

            var mockAttestationMehanism = this.mockRepository.Create<IAttestationMechanism>();

            _ = mockAttestationMehanism.Setup(c => c.GetAttestation())
                .Returns(new SymmetricKeyAttestation("", ""));

            _ = this.mockProvisioningServiceClient
                .Setup(c => c.GetEnrollmentGroupAttestationAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .ReturnsAsync(mockAttestationMehanism.Object);

            // Act
            var result = await manager.GetAttestation(modelName);

            // Assert
            Assert.IsNotNull(result);
            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task GetEnrollmentCredentialsAsyncShouldGetDPSAttestation(string modelName, string enrollmentGroupName)
        {
            // Arrange
            var manager = CreateManager();

            var mockAttestationMehanism = this.mockRepository.Create<IAttestationMechanism>();

            _ = mockAttestationMehanism.Setup(c => c.GetAttestation())
                .Returns(new SymmetricKeyAttestation("8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw==", "8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw=="));

            _ = this.mockProvisioningServiceClient
                .Setup(c => c.GetEnrollmentGroupAttestationAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .ReturnsAsync(mockAttestationMehanism.Object);

            _ = this.mockConfigHandler.SetupGet(c => c.DPSEndpoint)
                .Returns("FakeEndpoint");

            _ = this.mockConfigHandler.SetupGet(c => c.DPSScopeID)
                .Returns("FakeScopeID");

            // Act
            var result = await manager.GetEnrollmentCredentialsAsync(
                "sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6",
                modelName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6", result.RegistrationID);
            Assert.AreEqual("https://FakeEndpoint", result.ProvisioningEndpoint);
            Assert.AreEqual("FakeScopeID", result.ScopeID);
            Assert.AreEqual("Jsm0lyGpjaVYVP2g3FnmnmG9dI/9qU24wNoykUmermc=", result.SymmetricKey);
            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task WhenEnrollmentGoupsNotExistGetEnrollmentCredentialsAsyncShouldCreateNewOne(string modelName, string enrollmentGroupName)
        {
            // Arrange
            var manager = CreateManager();
            EnrollmentGroup enrollmentGroup = null;

            var mockAttestationMehanism = this.mockRepository.Create<IAttestationMechanism>();

            _ = mockAttestationMehanism.Setup(c => c.GetAttestation())
                .Returns(new SymmetricKeyAttestation("8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw==", "8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw=="));

            _ = this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .Throws(new HttpRequestException(null, null, HttpStatusCode.NotFound));

            _ = this.mockProvisioningServiceClient
                .Setup(c => c.GetEnrollmentGroupAttestationAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .Throws(new HttpRequestException(null, null, HttpStatusCode.NotFound));

            _ = this.mockConfigHandler.SetupGet(c => c.DPSEndpoint)
                .Returns("FakeEndpoint");

            _ = this.mockConfigHandler.SetupGet(c => c.DPSScopeID)
                .Returns("FakeScopeID");

            _ = this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
                It.Is<EnrollmentGroup>(x => x.EnrollmentGroupId == enrollmentGroupName && x.Attestation is SymmetricKeyAttestation)))
                .Callback(() =>
                {
                    // When the enrollment group is created, the DPS can return the attestation.
                    _ = this.mockProvisioningServiceClient
                         .Setup(c => c.GetEnrollmentGroupAttestationAsync(It.Is<string>(x => x == enrollmentGroupName)))
                         .ReturnsAsync(mockAttestationMehanism.Object);
                })
                .ReturnsAsync((EnrollmentGroup e) =>
                {
                    enrollmentGroup = e;
                    return e;
                });

            // Act
            var result = await manager.GetEnrollmentCredentialsAsync(
                "sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6",
                modelName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6", result.RegistrationID);
            Assert.AreEqual("https://FakeEndpoint", result.ProvisioningEndpoint);
            Assert.AreEqual("FakeScopeID", result.ScopeID);
            Assert.AreEqual("Jsm0lyGpjaVYVP2g3FnmnmG9dI/9qU24wNoykUmermc=", result.SymmetricKey);

            this.mockRepository.VerifyAll();
        }
    }
}
