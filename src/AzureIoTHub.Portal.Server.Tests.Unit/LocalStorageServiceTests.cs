// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client;
    using Microsoft.JSInterop;
    using Microsoft.JSInterop.Infrastructure;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LocalStorageServiceTests
    {
        private MockRepository mockRepository;
        private Mock<IJSRuntime> mockJSRuntime;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockJSRuntime = this.mockRepository.Create<IJSRuntime>();
        }

        private LocalStorageService CreateService()
        {
            return new LocalStorageService(this.mockJSRuntime.Object);
        }

        [Test]
        public async Task GetFromLocalStorageCanDeserializeDictionary()
        {
            // Arrange
            var service = this.CreateService();
            var key = Guid.NewGuid().ToString();

            _ = this.mockJSRuntime.Setup(c => c.InvokeAsync<string>("localStorage.getItem", new object?[] { key }))
                .ReturnsAsync(/*lang=json,strict*/ "{\"item1\":true,\"item2\":false}");

            // Act
            var result = await service.GetFromLocalStorage<Dictionary<string, bool>>(key);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result["item1"]);
            Assert.IsFalse(result["item2"]);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenNotSetGetFromLocalStorageShouldReturnNull()
        {
            // Arrange
            var service = this.CreateService();
            var key = Guid.NewGuid().ToString();

            _ = this.mockJSRuntime.Setup(c => c.InvokeAsync<string>("localStorage.getItem", new object?[] { key }))
                .ReturnsAsync((string)null);

            // Act
            var result = await service.GetFromLocalStorage<Dictionary<string, bool>>(key);

            // Assert
            Assert.IsNull(result);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task SetLocalStorageCanSerializeDictionary()
        {
            // Arrange
            var service = this.CreateService();
            var key = Guid.NewGuid().ToString();

            var dictionary = new Dictionary<string, bool>
            {
                { "item1", true },
                { "item2", false }
            };

            _ = this.mockJSRuntime
                .Setup(c => c.InvokeAsync<IJSVoidResult>("localStorage.setItem", new object?[] { key, /*lang=json,strict*/ "{\"item1\":true,\"item2\":false}" }))
                .ReturnsAsync(new JSVoidResult());

            // Act
            await service.SetLocalStorage(key, dictionary);

            // Assert
            this.mockRepository.VerifyAll();
        }

        private class JSVoidResult: IJSVoidResult
        {

        }
    }
}
