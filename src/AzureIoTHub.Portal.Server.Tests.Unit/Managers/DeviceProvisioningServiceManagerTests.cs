using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Wrappers;
using Microsoft.Azure.Devices.Provisioning.Service;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static AzureIoTHub.Portal.Server.Startup;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Managers
{
    [TestFixture]
    public class DeviceProvisioningServiceManagerTests
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
            this.mockConfigHandler = this.mockRepository.Create<DevelopmentConfigHandler>(mockConfiguration.Object);
        }

        private DeviceProvisioningServiceManager CreateManager()
        {
            return new DeviceProvisioningServiceManager(
                this.mockProvisioningServiceClient.Object,
                this.mockConfigHandler.Object);
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task CreateEnrollmentGroupAsync_Should_Create_A_New_Enrollment_Group(string deviceType, string enrollmentGroupName)
        {
            // Arrange
            var manager = this.CreateManager();
            EnrollmentGroup enrollmentGroup = null;

            this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .Throws(new HttpRequestException(null, null, HttpStatusCode.NotFound));

            this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
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
        public async Task When_EnrollmentGroup_Already_Exist_CreateEnrollmentGroupAsync_Should_Update_Enrollment_Group(string deviceType, string enrollmentGroupName)
        {
            // Arrange
            var manager = this.CreateManager();
            EnrollmentGroup enrollmentGroup = null;
            var attestation = new SymmetricKeyAttestation("aaa", "bbb");

            this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .ReturnsAsync(new EnrollmentGroup (enrollmentGroupName, attestation)
                {
                    Capabilities = new DeviceCapabilities
                    {
                        IotEdge = false
                    }
                });

            this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
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
        public async Task CreateEnrollmentGroupFormModelAsync_Should_Create_A_New_Enrollment_Group(string modelName, string enrollmentGroupName)
        {
            // Arrange
            var manager = this.CreateManager();
            string modelId = "bbb";
            TwinCollection desiredProperties = new TwinCollection();
            EnrollmentGroup enrollmentGroup = null;

            this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .Throws(new HttpRequestException(null, null, HttpStatusCode.NotFound));

            this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
                It.Is<EnrollmentGroup>(x =>
                x.EnrollmentGroupId == enrollmentGroupName &&
                x.Attestation is SymmetricKeyAttestation)))
                .ReturnsAsync((EnrollmentGroup e) =>
                {
                    enrollmentGroup = e;
                    return e;
                });

            // Act
            var result = await manager.CreateEnrollmentGroupFormModelAsync(
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
            Assert.IsTrue(enrollmentGroup.InitialTwinState.Tags.Contains("deviceType"));
            Assert.AreEqual(modelName, enrollmentGroup.InitialTwinState.Tags["deviceType"].ToString());
            Assert.AreEqual(desiredProperties, enrollmentGroup.InitialTwinState.DesiredProperties);

            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task When_EnrollmentGroup_Exist_CreateEnrollmentGroupFormModelAsync_Should_Update(string modelName, string enrollmentGroupName)
        {
            // Arrange
            var manager = this.CreateManager();
            string modelId = "bbb";
            TwinCollection desiredProperties = new TwinCollection();
            EnrollmentGroup enrollmentGroup = null;

            var attestation = new SymmetricKeyAttestation("aaa", "bbb");

            this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .ReturnsAsync(new EnrollmentGroup(enrollmentGroupName, attestation)
                {
                    Capabilities = new DeviceCapabilities
                    {
                        IotEdge = true
                    }
                });

            this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
                It.Is<EnrollmentGroup>(x =>
                x.EnrollmentGroupId == enrollmentGroupName &&
                x.Attestation is SymmetricKeyAttestation)))
                .ReturnsAsync((EnrollmentGroup e) =>
                {
                    enrollmentGroup = e;
                    return e;
                });

            // Act
            var result = await manager.CreateEnrollmentGroupFormModelAsync(
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
            Assert.IsTrue(enrollmentGroup.InitialTwinState.Tags.Contains("deviceType"));
            Assert.AreEqual(modelName, enrollmentGroup.InitialTwinState.Tags["deviceType"].ToString());
            Assert.AreEqual(desiredProperties, enrollmentGroup.InitialTwinState.DesiredProperties);
            Assert.IsAssignableFrom<SymmetricKeyAttestation>(result.Attestation);
            Assert.AreEqual(attestation, result.Attestation);

            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task GetAttestation_Should_Return_DPS_Attestation(string modelName, string enrollmentGroupName)
        {
            // Arrange
            var manager = this.CreateManager();

            var mockAttestationMehanism = this.mockRepository.Create<IAttestationMechanism>();

            mockAttestationMehanism.Setup(c => c.GetAttestation())
                .Returns(new SymmetricKeyAttestation("", ""));

            this.mockProvisioningServiceClient
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
        public async Task GetEnrollmentCredentialsAsync_Should_Get_DPS_Attestation(string modelName, string enrollmentGroupName)
        {
            // Arrange
            var manager = this.CreateManager();

            var mockAttestationMehanism = this.mockRepository.Create<IAttestationMechanism>();

            mockAttestationMehanism.Setup(c => c.GetAttestation())
                .Returns(new SymmetricKeyAttestation("8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw==", "8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw=="));

            this.mockProvisioningServiceClient
                .Setup(c => c.GetEnrollmentGroupAttestationAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .ReturnsAsync(mockAttestationMehanism.Object);

            this.mockConfigHandler.SetupGet(c => c.DPSIDScope)
                .Returns("FakeIDScope");

            this.mockConfigHandler.SetupGet(c => c.DPSEndpoint)
                .Returns("FakeEndpoint");

            // Act
            var result = await manager.GetEnrollmentCredentialsAsync(
                "sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6",
                modelName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("sn-007-888-abc-mac-a1-b2-c3-d4-e5-f6", result.RegistrationID);
            Assert.AreEqual("FakeIDScope", result.ScopeID);
            Assert.AreEqual("FakeEndpoint", result.ProvisioningEndpoint);
            Assert.AreEqual("Jsm0lyGpjaVYVP2g3FnmnmG9dI/9qU24wNoykUmermc=", result.SymmetricKey);
            this.mockRepository.VerifyAll();
        }

        [TestCase("aaa", "aaa")]
        [TestCase("AAA", "aaa")]
        [TestCase("AAA AAA", "aaa-aaa")]
        public async Task When_Enrollment_Goups_Not_Exist_GetEnrollmentCredentialsAsync_Should_CreateNew_One(string modelName, string enrollmentGroupName)
        {
            // Arrange
            var manager = this.CreateManager();
            EnrollmentGroup enrollmentGroup = null;

            var mockAttestationMehanism = this.mockRepository.Create<IAttestationMechanism>();

            mockAttestationMehanism.Setup(c => c.GetAttestation())
                .Returns(new SymmetricKeyAttestation("8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw==", "8isrFI1sGsIlvvFSSFRiMfCNzv21fjbE/+ah/lSh3lF8e2YG1Te7w1KpZhJFFXJrqYKi9yegxkqIChbqOS9Egw=="));

            this.mockProvisioningServiceClient.Setup(c => c.GetEnrollmentGroupAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .Throws(new HttpRequestException(null, null, HttpStatusCode.NotFound));

            this.mockProvisioningServiceClient
                .Setup(c => c.GetEnrollmentGroupAttestationAsync(It.Is<string>(x => x == enrollmentGroupName)))
                .Throws(new HttpRequestException(null, null, HttpStatusCode.NotFound));

            this.mockConfigHandler.SetupGet(c => c.DPSIDScope)
                .Returns("FakeIDScope");

            this.mockConfigHandler.SetupGet(c => c.DPSEndpoint)
                .Returns("FakeEndpoint");

            this.mockProvisioningServiceClient.Setup(c => c.CreateOrUpdateEnrollmentGroupAsync(
                It.Is<EnrollmentGroup>(x => x.EnrollmentGroupId == enrollmentGroupName && x.Attestation is SymmetricKeyAttestation)))
                .Callback(() =>
                {
                    // When the enrollment group is created, the DPS can return the attestation.
                    this.mockProvisioningServiceClient
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
            Assert.AreEqual("FakeIDScope", result.ScopeID);
            Assert.AreEqual("FakeEndpoint", result.ProvisioningEndpoint);
            Assert.AreEqual("Jsm0lyGpjaVYVP2g3FnmnmG9dI/9qU24wNoykUmermc=", result.SymmetricKey);

            this.mockRepository.VerifyAll();
        }
    }
}
