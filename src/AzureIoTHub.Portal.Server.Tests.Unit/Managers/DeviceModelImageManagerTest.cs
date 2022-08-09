// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Managers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using AzureIoTHub.Portal.Server.Exceptions;
    using AzureIoTHub.Portal.Server.Managers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceModelImageManagerTest : BackendUnitTest
    {
        private Mock<BlobServiceClient> mockBlobServiceClient;
        private Mock<BlobContainerClient> mockBlobContainerClient;
        private Mock<BlobClient> mockBlobClient;

        private IDeviceModelImageManager deviceModelImageManager;

        public override void Setup()
        {
            base.Setup();

            this.mockBlobServiceClient = MockRepository.Create<BlobServiceClient>();
            this.mockBlobContainerClient = MockRepository.Create<BlobContainerClient>();
            this.mockBlobClient = MockRepository.Create<BlobClient>();

            _ = ServiceCollection.AddSingleton(this.mockBlobServiceClient.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelImageManager, DeviceModelImageManager>();

            Services = ServiceCollection.BuildServiceProvider();

            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.mockBlobContainerClient.Object);

            _ = this.mockBlobContainerClient
                .Setup(x => x.SetAccessPolicy(It.IsAny<PublicAccessType>(),
                    It.IsAny<IEnumerable<BlobSignedIdentifier>>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>())).Returns(Response.FromValue(BlobsModelFactory.BlobContainerInfo(ETag.All, DateTimeOffset.Now), Mock.Of<Response>()));

            _ = this.mockBlobContainerClient.Setup(x => x.CreateIfNotExists(It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobContainerEncryptionScopeOptions>(),
                It.IsAny<CancellationToken>())).Returns(Response.FromValue(BlobsModelFactory.BlobContainerInfo(ETag.All, DateTimeOffset.Now), Mock.Of<Response>()));

            this.deviceModelImageManager = Services.GetRequiredService<IDeviceModelImageManager>();
        }

        [Test]
        public async Task ChangeDeviceModelImageShouldUploadImageAndReturnItsUri()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var expectedImageUri = Fixture.Create<Uri>();
            using var imageAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Fixture.Create<string>()));

            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.mockBlobContainerClient.Object);

            _ = this.mockBlobContainerClient
                .Setup(x => x.GetBlobClient(deviceModelId))
                .Returns(this.mockBlobClient.Object);

            _ = this.mockBlobClient
                .Setup(client =>
                    client.SetHttpHeadersAsync(It.IsAny<BlobHttpHeaders>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(BlobsModelFactory.BlobInfo(ETag.All, DateTimeOffset.Now), Mock.Of<Response>()));

            _ = this.mockBlobClient
                .Setup(client => client.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(
                    BlobsModelFactory.BlobContentInfo(ETag.All, DateTimeOffset.Now, Array.Empty<byte>(), string.Empty,
                        1L), Mock.Of<Response>()));

            _ = this.mockBlobClient
                .Setup(client => client.Uri)
                .Returns(expectedImageUri);

            // Act
            var result = await this.deviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, imageAsMemoryStream);

            // Assert
            _ = result.Should().Be(expectedImageUri.ToString());
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeleteAsyncFailedDeleteDeviceModelImageAsyncShouldThrowAnInternalServerErrorException()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var imageUri = Fixture.Create<Uri>();

            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.mockBlobContainerClient.Object);

            _ = this.mockBlobContainerClient
                .Setup(x => x.GetBlobClient(deviceModelId))
                .Returns(this.mockBlobClient.Object);

            _ = this.mockBlobClient
                .Setup(client => client.Uri)
                .Returns(imageUri);

            _ = this.mockBlobClient
                .Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(""));

            // Act
            var act = async () => await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SyncImagesCacheControlShouldUpdateBlobsCacheControls()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();

            var blob = BlobsModelFactory.BlobItem(name:deviceModelId);
            var blobsPage = Page<BlobItem>.FromValues(new[] { blob }, default, new Mock<Response>().Object);
            var blobsPageable = AsyncPageable<BlobItem>.FromPages(new[] { blobsPage });

            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.mockBlobContainerClient.Object);

            _ = this.mockBlobContainerClient
                .Setup(x => x.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(blobsPageable);

            _ = this.mockBlobContainerClient
                .Setup(x => x.GetBlobClient(deviceModelId))
                .Returns(this.mockBlobClient.Object);

            _ = this.mockBlobClient
                .Setup(client =>
                    client.SetHttpHeadersAsync(It.IsAny<BlobHttpHeaders>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(BlobsModelFactory.BlobInfo(ETag.All, DateTimeOffset.Now), Mock.Of<Response>()));

            // Act
            await this.deviceModelImageManager.SyncImagesCacheControl();

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
