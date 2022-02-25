using AzureIoTHub.Portal.Server.Managers;
using Microsoft.Azure.Devices.Provisioning.Service;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Managers
{
    [TestFixture]
    public class ConnectionStringManagerTests
    {
        private MockRepository mockRepository;

        private Mock<IDeviceProvisioningServiceManager> mockDeviceProvisioningServiceManager;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDeviceProvisioningServiceManager = this.mockRepository.Create<IDeviceProvisioningServiceManager>();
        }

        private ConnectionStringManager CreateManager()
        {
            return new ConnectionStringManager(
                this.mockDeviceProvisioningServiceManager.Object);
        }

        private static string GenerateKey()
        {
            var length = 48;
            var rnd = RandomNumberGenerator.GetBytes(length);

            return Convert.ToBase64String(rnd);
        }

        [Test]
        public async Task GetSymmetricKey_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var manager = this.CreateManager();
            string deviceId = "aaaa";
            string deviceType = "bbb";

            SymmetricKeyAttestation attestation = new SymmetricKeyAttestation(GenerateKey(), GenerateKey());

            this.mockDeviceProvisioningServiceManager
                .Setup(x => x.GetAttestation(It.Is<string>(c => c == deviceType)))
                .ReturnsAsync(attestation);
               
            // Act
            var result = await manager.GetSymmetricKey(
                deviceId,
                deviceType);

            // Assert
            this.mockRepository.VerifyAll();
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(attestation.PrimaryKey));
            Assert.AreEqual(Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId))), result);
        }
    }
}
