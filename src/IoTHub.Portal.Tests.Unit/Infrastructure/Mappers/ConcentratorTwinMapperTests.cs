// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Crosscutting.Extensions;
    using IoTHub.Portal.Infrastructure.Mappers;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NUnit.Framework;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class ConcentratorTwinMapperTests : IDisposable
    {
        private MockRepository mockRepository;

#pragma warning disable CA2213 // Disposable fields should be disposed
        private HttpClient mockHttpClient;
#pragma warning restore CA2213 // Disposable fields should be disposed

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

        private static ConcentratorTwinMapper CreateConcentratorTwinMapper()
        {
            return new ConcentratorTwinMapper();
        }

        [Test]
        public void CreateDeviceDetailsStateUnderTestExpectedBehavior()
        {
            // Arrange
            var concentratorTwinMapper = CreateConcentratorTwinMapper();
            var twin = new Twin
            {
                DeviceId = Guid.NewGuid().ToString()
            };

            twin.Tags[nameof(ConcentratorDto.DeviceType).ToCamelCase()] = Guid.NewGuid().ToString();
            twin.Tags[nameof(ConcentratorDto.DeviceName).ToCamelCase()] = Guid.NewGuid().ToString();
            twin.Tags[nameof(ConcentratorDto.LoraRegion).ToCamelCase()] = Guid.NewGuid().ToString();

            twin.Properties.Reported["DevAddr"] = Guid.NewGuid().ToString();

            twin.Properties.Desired[nameof(ConcentratorDto.ClientThumbprint).ToCamelCase()] = new List<string>() { Guid.NewGuid().ToString() };

            // Act
            var result = concentratorTwinMapper.CreateDeviceDetails(twin);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsFalse(result.IsConnected);
            Assert.IsFalse(result.IsEnabled);

            Assert.AreEqual(twin.Tags[nameof(ConcentratorDto.DeviceName).ToCamelCase()].ToString(), result.DeviceName);
            Assert.AreEqual(twin.Tags[nameof(ConcentratorDto.LoraRegion).ToCamelCase()].ToString(), result.LoraRegion);
            Assert.AreEqual(twin.Tags[nameof(ConcentratorDto.DeviceType).ToCamelCase()].ToString(), result.DeviceType);

            Assert.IsTrue(result.AlreadyLoggedInOnce);

            Assert.AreEqual(twin.Properties.Desired[nameof(ConcentratorDto.ClientThumbprint).ToCamelCase()][0].ToString(), result.ClientThumbprint);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateTwinStateUnderTestExpectedBehavior()
        {
            // Arrange
            var concentratorTwinMapper = CreateConcentratorTwinMapper();

            var twin = new Twin();

            var item = new ConcentratorDto
            {
                LoraRegion = Guid.NewGuid().ToString(),
                DeviceName = Guid.NewGuid().ToString(),
                DeviceType = Guid.NewGuid().ToString(),
                ClientThumbprint = Guid.NewGuid().ToString(),
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

            twin.Properties.Desired[nameof(ConcentratorDto.ClientThumbprint).ToCamelCase()] = new List<string>() { item.ClientThumbprint };

            // Act
            concentratorTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.AreEqual(item.DeviceName, twin.Tags[nameof(ConcentratorDto.DeviceName).ToCamelCase()].ToString());
            Assert.AreEqual(item.DeviceType, twin.Tags[nameof(ConcentratorDto.DeviceType).ToCamelCase()].ToString());
            Assert.AreEqual(item.LoraRegion, twin.Tags[nameof(ConcentratorDto.LoraRegion).ToCamelCase()].ToString());

            Assert.AreEqual(item.ClientThumbprint, twin.Properties.Desired[nameof(ConcentratorDto.ClientThumbprint).ToCamelCase()][0].ToString());

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenClientThumbprintNotProvidedTheDesiredPropertyShouldBeNull()
        {
            // Arrange
            var concentratorTwinMapper = CreateConcentratorTwinMapper();

            var twin = new Twin();
            twin.Properties.Desired[nameof(ConcentratorDto.ClientThumbprint).ToCamelCase()] = new List<string>() { Guid.NewGuid().ToString() };

            var item = new ConcentratorDto();

            // Act
            concentratorTwinMapper.UpdateTwin(twin, item);

            // Assert
            Assert.IsNull(twin.Properties.Desired[nameof(ConcentratorDto.ClientThumbprint).ToCamelCase()].Value);

            this.mockRepository.VerifyAll();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
