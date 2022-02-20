using AzureIoTHub.Portal.Server.Extensions;
using AzureIoTHub.Portal.Server.Helpers;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Text;

namespace AzureIoTHub.Portal.Server.Tests.Mappers
{
    [TestFixture]
    public class ConcentratorTwinMapperTests
    {
        private MockRepository mockRepository;

        private HttpClient mockHttpClient;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<HttpMessageHandler> httpMessageHandlerMock;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.httpMessageHandlerMock = this.mockRepository.Create<HttpMessageHandler>();
            this.mockHttpClient = new HttpClient(this.httpMessageHandlerMock.Object);
        }

        private ConcentratorTwinMapper CreateConcentratorTwinMapper()
        {
            return new ConcentratorTwinMapper();
        }

        [Test]
        public void CreateDeviceDetails_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var concentratorTwinMapper = this.CreateConcentratorTwinMapper();
            Twin twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            twin.Tags[nameof(Concentrator.DeviceType).ToCamelCase()] = Guid.NewGuid().ToString();
            twin.Tags[nameof(Concentrator.DeviceName).ToCamelCase()] = Guid.NewGuid().ToString();
            twin.Tags[nameof(Concentrator.LoraRegion).ToCamelCase()] = Guid.NewGuid().ToString();

            twin.Properties.Reported["DevAddr"] = Guid.NewGuid().ToString();

            twin.Properties.Desired[nameof(Concentrator.ClientCertificateThumbprint)] = Guid.NewGuid().ToString();

            // Act
            var result = concentratorTwinMapper.CreateDeviceDetails(twin);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(twin.Tags[nameof(Concentrator.DeviceName).ToCamelCase()].ToString(), result.DeviceName);
            Assert.AreEqual(twin.Tags[nameof(Concentrator.LoraRegion).ToCamelCase()].ToString(), result.LoraRegion);
            Assert.AreEqual(twin.Tags[nameof(Concentrator.DeviceType).ToCamelCase()].ToString(), result.DeviceType);

            Assert.IsTrue(result.AlreadyLoggedInOnce);

            Assert.AreEqual(twin.Properties.Desired[nameof(Concentrator.ClientCertificateThumbprint)].ToString(), result.ClientCertificateThumbprint);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTwin_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var concentratorTwinMapper = this.CreateConcentratorTwinMapper();

            Twin twin = new Twin();

            Concentrator item = new Concentrator
            {
                LoraRegion = Guid.NewGuid().ToString(),
                DeviceName = Guid.NewGuid().ToString(),
                DeviceType = Guid.NewGuid().ToString(),
                ClientCertificateThumbprint = Guid.NewGuid().ToString(),
                IsConnected = false,
                IsEnabled = false,
                AlreadyLoggedInOnce = false,
                RouterConfig = new RouterConfig()
            };

            using var deviceResponseMock = new HttpResponseMessage();

            deviceResponseMock.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            // this.httpMessageHandlerMock
            //     .Protected()
            //     .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.LocalPath.Equals($"/{item.LoraRegion}.json", StringComparison.OrdinalIgnoreCase)), ItExpr.IsAny<CancellationToken>())
            //     .ReturnsAsync((HttpRequestMessage req, CancellationToken token) => deviceResponseMock)
            //     .Verifiable();
            
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceType), item.DeviceType);
            DeviceHelper.SetTagValue(twin, nameof(item.LoraRegion), item.LoraRegion);

            twin.Properties.Desired[nameof(Concentrator.ClientCertificateThumbprint)] = item.ClientCertificateThumbprint;

            // Act
            concentratorTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.DeviceName, twin.Tags[nameof(Concentrator.DeviceName).ToCamelCase()].ToString());
            Assert.AreEqual(item.DeviceType, twin.Tags[nameof(Concentrator.DeviceType).ToCamelCase()].ToString());
            Assert.AreEqual(item.LoraRegion, twin.Tags[nameof(Concentrator.LoraRegion).ToCamelCase()].ToString());

            Assert.AreEqual(item.ClientCertificateThumbprint, twin.Properties.Desired[nameof(Concentrator.ClientCertificateThumbprint)].ToString());

            this.mockRepository.VerifyAll();
        }
    }
}
