// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Managers
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using FluentAssertions;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Options;
    using IoTHub.Portal.Infrastructure.Managers;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceModelImageManagerTest : BackendUnitTest
    {
        private Mock<BlobServiceClient> mockBlobServiceClient;
        private Mock<BlobContainerClient> mockBlobContainerClient;
        private Mock<BlobClient> mockBlobClient;
        private Mock<ConfigHandler> mockConfigHandler;
        private Mock<IOptions<DeviceModelImageOptions>> mockDeviceModelImageOptions;

        private IDeviceModelImageManager deviceModelImageManager;

        public override void Setup()
        {
            base.Setup();

            this.mockBlobServiceClient = MockRepository.Create<BlobServiceClient>();
            this.mockBlobContainerClient = MockRepository.Create<BlobContainerClient>();
            this.mockBlobClient = MockRepository.Create<BlobClient>();
            this.mockConfigHandler = MockRepository.Create<ConfigHandler>();
            this.mockDeviceModelImageOptions = MockRepository.Create<IOptions<DeviceModelImageOptions>>();

            _ = ServiceCollection.AddSingleton(this.mockBlobServiceClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageOptions.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigHandler.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelImageManager, DeviceModelImageManager>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelImageManager = Services.GetRequiredService<IDeviceModelImageManager>();
        }

        [Test]
        public async Task ChangeDeviceModelImageShouldUploadImageAndReturnItsUri()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var expectedImageUri = Fixture.Create<Uri>();
            using var imageAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Fixture.Create<string>()));

            var mockOptions = new DeviceModelImageOptions()
            {
                BaseUri = Fixture.Create<Uri>()
            };

            _ = this.mockDeviceModelImageOptions.Setup(x => x.Value).Returns(mockOptions);

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

            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(3600);

            // Act
            var result = await this.deviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, imageAsMemoryStream);

            // Assert
            _ = result.Should().Be(expectedImageUri.ToString());
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ChangeDeviceModelImageShouldSetDefaultImageToModel()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var expectedImageUri = Fixture.Create<Uri>();
            using var imageAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Fixture.Create<string>()));

            var mockOptions = new DeviceModelImageOptions()
            {
                BaseUri = Fixture.Create<Uri>()
            };

            _ = this.mockDeviceModelImageOptions.Setup(x => x.Value).Returns(mockOptions);

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

            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(3600);

            // Act
            var result = await this.deviceModelImageManager.SetDefaultImageToModel(deviceModelId);

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

            var mockOptions = new DeviceModelImageOptions()
            {
                BaseUri = imageUri
            };

            _ = this.mockDeviceModelImageOptions.Setup(x => x.Value).Returns(mockOptions);

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

            var mockOptions = new DeviceModelImageOptions()
            {
                BaseUri = Fixture.Create<Uri>()
            };

            _ = this.mockDeviceModelImageOptions.Setup(x => x.Value).Returns(mockOptions);

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

            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(3600);

            // Act
            await this.deviceModelImageManager.SyncImagesCacheControl();

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void ComputeImageUriShouldReturnTheRightUri()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var imageUri = Fixture.Create<Uri>();

            var mockOptions = new DeviceModelImageOptions()
            {
                BaseUri = imageUri
            };

            _ = this.mockDeviceModelImageOptions.Setup(x => x.Value).Returns(mockOptions);

            // Act
            var result = this.deviceModelImageManager.ComputeImageUri(deviceModelId);

            // Assert
            Assert.AreEqual($"{imageUri}/{deviceModelId}", result.ToString());
        }

        [Test]
        public async Task InitializeDefaultImageBlob()
        {
            // Arrange
            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.mockBlobContainerClient.Object);

            _ = this.mockBlobContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(this.mockBlobClient.Object);

            _ = this.mockBlobClient
                .Setup(client => client.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(
                    BlobsModelFactory.BlobContentInfo(ETag.All, DateTimeOffset.Now, Array.Empty<byte>(), string.Empty,
                        1L), Mock.Of<Response>()));

            var mockOptions = new DeviceModelImageOptions()
            {
                BaseUri = Fixture.Create<Uri>()
            };

            _ = this.mockDeviceModelImageOptions.Setup(x => x.Value).Returns(mockOptions);

            // Act
            await this.deviceModelImageManager.InitializeDefaultImageBlob();

            // Assert
            MockRepository.VerifyAll();
        }

    }
}
