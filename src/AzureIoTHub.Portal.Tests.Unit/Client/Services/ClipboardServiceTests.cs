// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Services
{
    using System;
    using System.Linq;
    using AzureIoTHub.Portal.Client.Services;
    using Microsoft.JSInterop;
    using Microsoft.JSInterop.Infrastructure;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ClipboardServiceTests
    {
        private MockRepository mockRepository;

        private Mock<IJSRuntime> mockJSRuntime;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockJSRuntime = this.mockRepository.Create<IJSRuntime>();
        }

        private ClipboardService CreateService()
        {
            return new ClipboardService(
                this.mockJSRuntime.Object);
        }

        [Test]
        public void WriteTextAsyncStateUnderTestExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var text = Guid.NewGuid().ToString();

            // Act
            _ = service.WriteTextAsync(text);

            // Assert
            this.mockJSRuntime.Verify(c => c.InvokeAsync<IJSVoidResult>(
                    It.Is<string>(x => x == "navigator.clipboard.writeText"),
                    It.Is<object[]>(x => x.Single().ToString() == text)), Times.Once);

            this.mockRepository.VerifyAll();
        }
    }
}
