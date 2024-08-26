// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Managers
{
    using Shared.Constants;

    [TestFixture]
    public class DeviceModelImageManagerTest : BackendUnitTest
    {
        private Mock<BlobServiceClient> mockBlobServiceClient;
        private Mock<BlobContainerClient> mockBlobContainerClient;
        private Mock<BlobClient> mockBlobClient;
        private Mock<ConfigHandler> mockConfigHandler;

        private IDeviceModelImageManager deviceModelImageManager;

        public override void Setup()
        {
            base.Setup();

            this.mockBlobServiceClient = MockRepository.Create<BlobServiceClient>();
            this.mockBlobContainerClient = MockRepository.Create<BlobContainerClient>();
            this.mockBlobClient = MockRepository.Create<BlobClient>();
            this.mockConfigHandler = MockRepository.Create<ConfigHandler>();

            _ = ServiceCollection.AddSingleton(this.mockBlobServiceClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockConfigHandler.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelImageManager, DeviceModelImageManager>();

            Services = ServiceCollection.BuildServiceProvider();

            this.deviceModelImageManager = Services.GetRequiredService<IDeviceModelImageManager>();
        }

        [Test]
        public async Task ChangeDeviceModelImageShouldUploadImageAndReturnItsValue()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var expectedImage = DeviceModelImageOptions.DefaultImage;
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
                .Setup(client => client.Name)
                .Returns(deviceModelId);

            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(3600);

            // Act
            var result = await this.deviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, expectedImage);

            // Assert
            _ = result.Should().Be(expectedImage);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ChangeDeviceModelImageShouldSetDefaultImageToModel()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var expectedImage = DeviceModelImageOptions.DefaultImage;
            var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(BinaryData.FromString(expectedImage));

            using var imageAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedImage));

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
                .Setup(client => client.Name)
                .Returns(deviceModelId);

            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(3600);

            // Act
            var result = await this.deviceModelImageManager.SetDefaultImageToModel(deviceModelId);

            // Assert
            _ = result.Should().Be(expectedImage);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenDeleteAsyncFailedDeleteDeviceModelImageAsyncShouldThrowAnInternalServerErrorException()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var image = DeviceModelImageOptions.DefaultImage;

            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.mockBlobContainerClient.Object);

            _ = this.mockBlobContainerClient
                .Setup(x => x.GetBlobClient(deviceModelId))
                .Returns(this.mockBlobClient.Object);

            _ = this.mockBlobClient
                .Setup(client => client.Name)
                .Returns(image);

            _ = this.mockBlobClient
                .Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(string.Empty));

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

            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(3600);

            // Act
            await this.deviceModelImageManager.SyncImagesCacheControl();

            // Assert
            MockRepository.VerifyAll();
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

            // Act
            await this.deviceModelImageManager.InitializeDefaultImageBlob();

            // Assert
            MockRepository.VerifyAll();
        }

    }
}
