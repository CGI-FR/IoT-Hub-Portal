// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class UnitOfWorkTests
    {
        private MockRepository mockRepository;
        private Mock<DbContext> mockDbContext;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDbContext = this.mockRepository.Create<DbContext>();
        }

        [TestCase]
        public async Task SaveAsyncTest()
        {
            // Arrange
            using var instance = new UnitOfWork<DbContext>(this.mockDbContext.Object);

            _ = this.mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            _ = this.mockDbContext.Setup(c => c.Dispose());

            // Act
            await instance.SaveAsync();

            // Assert
            this.mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase]
        public void DisposeTest()
        {
            // Arrange
            using var instance = new UnitOfWork<DbContext>(this.mockDbContext.Object);
            _ = this.mockDbContext.Setup(c => c.Dispose());

            // Act
            instance.Dispose();

            // Assert
            this.mockDbContext.Verify(c => c.Dispose(), Times.Once);
        }
    }
}
