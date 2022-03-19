// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Managers
{
    using AzureIoTHub.Portal.Server.Managers;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [TestFixture]
    public class RouterConfigManagerTests : IDisposable
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

        private RouterConfigManager CreateManager()
        {
            return new RouterConfigManager(this.mockHttpClient);
        }

        [Test]
        public async Task GetRouterConfigStateUnderTestExpectedBehavior()
        {
            // Arrange
            var routerConfig = CreateManager();
            var loraRegion = Guid.NewGuid().ToString();

            using var deviceResponseMock = new HttpResponseMessage();

            deviceResponseMock.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            this.httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.LocalPath.Equals($"/{loraRegion}.json", StringComparison.OrdinalIgnoreCase)), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => deviceResponseMock)
                .Verifiable();

            // Act
            var result = await routerConfig.GetRouterConfig(loraRegion);

            // Assert
            Assert.IsNotNull(result);
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
