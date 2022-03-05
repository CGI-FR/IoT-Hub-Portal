using AzureIoTHub.Portal.Client.Services;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using Microsoft.JSInterop.Infrastructure;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
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
        public void WriteTextAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = this.CreateService();
            string text = Guid.NewGuid().ToString();

            // Act
            var result = service.WriteTextAsync(text);

            // Assert
            this.mockJSRuntime.Verify(c => c.InvokeAsync<IJSVoidResult>(
                    It.Is<string>(x => x == "navigator.clipboard.writeText"),
                    It.Is<object?[]>(x => x.Single().ToString() == text)), Times.Once);

            this.mockRepository.VerifyAll();
        }
    }
}
