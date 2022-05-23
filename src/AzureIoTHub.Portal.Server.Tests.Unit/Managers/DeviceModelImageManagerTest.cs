// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using AzureIoTHub.Portal.Server.Exceptions;
    using AzureIoTHub.Portal.Server.Managers;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceModelImageManagerTest
    {
        private MockRepository mockRepository;

        private Mock<ILogger<DeviceModelImageManager>> mockLogger;
        private Mock<BlobServiceClient> mockBlobServiceClient;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockBlobServiceClient = this.mockRepository.Create<BlobServiceClient>();
            this.mockLogger = this.mockRepository.Create<ILogger<DeviceModelImageManager>>();
        }

        private DeviceModelImageManager CreateManager()
        {
            var mockBlobContainerClient = new Mock<BlobContainerClient>();

            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockBlobContainerClient.Object);
            _ = mockBlobContainerClient
                .Setup(x => x.SetAccessPolicy(It.IsAny<PublicAccessType>(),
                            It.IsAny<IEnumerable<BlobSignedIdentifier>>(),
                            It.IsAny<BlobRequestConditions>(),
                            It.IsAny<CancellationToken>()));
            _ = mockBlobContainerClient.Setup(x => x.CreateIfNotExists(It.IsAny<PublicAccessType>(),
                                                   It.IsAny<IDictionary<string, string>>(),
                                                   It.IsAny<BlobContainerEncryptionScopeOptions>(),
                                                   It.IsAny<CancellationToken>()));

            return new DeviceModelImageManager(this.mockLogger.Object, this.mockBlobServiceClient.Object);
        }

        [Test]
        public void WhenDeleteAsyncFAiletDeleteDeviceModelImageAsyncShouldThrowAnInternalServerErrorException()
        {
            // Arrange
            var mockDeviceModelImageManager = this.CreateManager();

            var mockBlobContainerClient = new Mock<BlobContainerClient>();
            var mockBlobClient = new Mock<BlobClient>();

            _ = this.mockLogger
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));


            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockBlobContainerClient.Object);

            _ = mockBlobContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(mockBlobClient.Object);

            _ = mockBlobClient
                .Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(""));

            // Act
            var act = async () => await mockDeviceModelImageManager.DeleteDeviceModelImageAsync("test");

            // Assert
            _ = act.Should().ThrowAsync<InternalServerErrorException>();

            this.mockRepository.VerifyAll();
        }
    }
}
