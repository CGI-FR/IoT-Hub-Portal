using AzureIoTHub.Portal.Server.Extensions;
using AzureIoTHub.Portal.Server.Mappers;
using AzureIoTHub.Portal.Shared.Models.Concentrator;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static AzureIoTHub.Portal.Server.Startup;

namespace AzureIoTHub.Portal.Server.Tests.Mappers
{
    [TestFixture]
    public class ConcentratorTwinMapperTests
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
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
            twin.Tags[nameof(Concentrator.DeviceFriendlyName).ToCamelCase()] = Guid.NewGuid().ToString();
            twin.Tags[nameof(Concentrator.LoraRegion).ToCamelCase()] = Guid.NewGuid().ToString();

            twin.Properties.Reported["DevAddr"] = Guid.NewGuid().ToString();

            twin.Properties.Desired[nameof(Concentrator.ClientCertificateThumbprint)] = Guid.NewGuid().ToString();

            // Act
            var result = concentratorTwinMapper.CreateDeviceDetails(twin);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(twin.Tags[nameof(Concentrator.DeviceFriendlyName).ToCamelCase()].ToString(), result.DeviceFriendlyName);
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
            //this.mockHttpClient.Setup(x => x.GetFromJsonAsync<RouterConfig>(It.IsAny<string>()))
            //    .ReturnsAsync(new RouterConfig())
            //    .Verifiable();

            Twin twin = new Twin();

            Concentrator item = new Concentrator
            {
                LoraRegion = Guid.NewGuid().ToString(),
                DeviceFriendlyName = Guid.NewGuid().ToString(),
                DeviceType = Guid.NewGuid().ToString(),
                ClientCertificateThumbprint = Guid.NewGuid().ToString(),
                IsConnected = false,
                IsEnabled = false,
                AlreadyLoggedInOnce = false,
                RouterConfig = new RouterConfig()
            };
            
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.DeviceFriendlyName), item.DeviceFriendlyName);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.DeviceType), item.DeviceType);
            Helpers.DeviceHelper.SetTagValue(twin, nameof(item.LoraRegion), item.LoraRegion);

            twin.Properties.Desired[nameof(Concentrator.ClientCertificateThumbprint)] = item.ClientCertificateThumbprint;

            // Act
            concentratorTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.DeviceFriendlyName, twin.Tags[nameof(Concentrator.DeviceFriendlyName).ToCamelCase()].ToString());
            Assert.AreEqual(item.DeviceType, twin.Tags[nameof(Concentrator.DeviceType).ToCamelCase()].ToString());
            Assert.AreEqual(item.LoraRegion, twin.Tags[nameof(Concentrator.LoraRegion).ToCamelCase()].ToString());

            Assert.AreEqual(item.ClientCertificateThumbprint, twin.Properties.Desired[nameof(Concentrator.ClientCertificateThumbprint)].ToString());

            this.mockRepository.VerifyAll();
        }
    }
}
