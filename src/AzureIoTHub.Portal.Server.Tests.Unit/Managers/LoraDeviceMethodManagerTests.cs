// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Managers
{
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using FluentAssertions;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [TestFixture]
    public class LoraDeviceMethodManagerTests : IDisposable
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private HttpClient mockHttpClient;
#pragma warning restore CA2213 // Disposable fields should be disposed
        private MockRepository mockRepository;

        private Mock<HttpMessageHandler> httpMessageHandlerMock;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.httpMessageHandlerMock = this.mockRepository.Create<HttpMessageHandler>();
            this.mockHttpClient = new HttpClient(this.httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://fake.local")
            };
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageThrowsArgumentNullExceptionWhenDeviceIdIsNull()
        {
            // Arrange
            var loraDeviceMethodManager = new LoraDeviceMethodManager(this.mockHttpClient);

            // Act
            var act = () => loraDeviceMethodManager.ExecuteLoRaDeviceMessage(null, null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageThrowsArgumentNullExceptionWhenCommandIsNull()
        {
            // Arrange
            var loraDeviceMethodManager = new LoraDeviceMethodManager(this.mockHttpClient);
            var deviceId = Guid.NewGuid().ToString();

            // Act
            var act = () => loraDeviceMethodManager.ExecuteLoRaDeviceMessage(deviceId, null);

            // Assert
            _ = await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task ExecuteLoRaDeviceMessageMustBeSuccessfullWhenParametersAreProvided()
        {
            // Arrange
            var loraDeviceMethodManager = new LoraDeviceMethodManager(this.mockHttpClient);
            var deviceId = Guid.NewGuid().ToString();
            var command = new DeviceModelCommand {
                Frame = Guid.NewGuid().ToString()
            };

            using var deviceResponseMock = new HttpResponseMessage();

            deviceResponseMock.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            this.httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.LocalPath.Equals($"/api/cloudtodevicemessage/{deviceId}", StringComparison.OrdinalIgnoreCase) && req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => deviceResponseMock)
                .Verifiable();

            // Act
            var result = await loraDeviceMethodManager.ExecuteLoRaDeviceMessage(deviceId, command);

            // Assert
            _ = result.Should().NotBeNull();
            _ = result.IsSuccessStatusCode.Should().BeTrue();
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
