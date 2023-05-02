// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Managers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using AutoFixture;
    using Azure;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Options;
    using AzureIoTHub.Portal.Infrastructure.Managers;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AwsDeviceModelImageManagerTest : BackendUnitTest
    {
        private Mock<ConfigHandler> mockConfigHandler;
        private Mock<IOptions<DeviceModelImageOptions>> mockDeviceModelImageOptions;
        private Mock<IAmazonS3> s3ClientMock;
        private Mock<PutObjectResponse> putObjectResponse;
        private Mock<PutObjectRequest> putObjectRequest;
        private Mock<PutACLRequest> putACL;
        private IDeviceModelImageManager awsDeviceModelImageManager;



        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelImageOptions = MockRepository.Create<IOptions<DeviceModelImageOptions>>();
            this.mockConfigHandler = MockRepository.Create<ConfigHandler>();

            this.s3ClientMock = MockRepository.Create<IAmazonS3>();
            this.putObjectRequest = MockRepository.Create<PutObjectRequest>();
            this.putObjectResponse = MockRepository.Create<PutObjectResponse>();
            this.putACL = MockRepository.Create<PutACLRequest>();

            _ = ServiceCollection.AddSingleton(this.mockConfigHandler.Object);
            _ = ServiceCollection.AddSingleton(this.s3ClientMock.Object);
            _ = ServiceCollection.AddSingleton(this.putObjectRequest.Object);
            _ = ServiceCollection.AddSingleton<IDeviceModelImageManager, AwsDeviceModelImageManager>();

            Services = ServiceCollection.BuildServiceProvider();

            this.awsDeviceModelImageManager = Services.GetRequiredService<IDeviceModelImageManager>();
        }

        /*===========================*** Tests for ChangeDeviceModelImageAsync() **===========================*/
        [Test]
        public async Task ChangeDeviceModelImageShouldUploadImageAndReturnAUri()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            using var imageAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Fixture.Create<string>()));
            var bucketName = Fixture.Create<string>();
            var region = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns(region);
            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());


            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.s3ClientMock.Setup(s3 => s3.PutACLAsync(It.IsAny<PutACLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutACLResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            var expectedRetunUrl = $"https://{bucketName}.s3.{RegionEndpoint.GetBySystemName(region)}.amazonaws.com/{deviceModelId}";


            // Act
            var result = await this.awsDeviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, imageAsMemoryStream);

            // Assert
            Assert.NotNull(result);
            _ = result.Should().Be(expectedRetunUrl);
            this.s3ClientMock.VerifyAll();
        }

        [Test]
        public void ChangeDeviceModelImageShouldThrowsAnExceptionForPutObjectResStatusCode()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            using var imageAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Fixture.Create<string>()));
            var bucketName = "invalid bucket Name for example";
            var region = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns(region);
            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());


            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.BadGateway
                });


            // Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            {
                // Act
                _ = await this.awsDeviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, imageAsMemoryStream);

            }, "Error by uploading the image in S3 Storage");
            this.s3ClientMock.VerifyAll();
        }

        [Test]
        public void ChangeDeviceModelImageShouldThrowsAnExceptionForPutACLStatusCode()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            using var imageAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Fixture.Create<string>()));
            var bucketName = "invalid bucket Name for example";
            var region = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns(region);
            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());


            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });
            _ = this.s3ClientMock.Setup(s3 => s3.PutACLAsync(It.IsAny<PutACLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutACLResponse
                {
                    HttpStatusCode = HttpStatusCode.BadGateway
                });

            // Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            {
                // Act
                _ = await this.awsDeviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, imageAsMemoryStream);

            }, "Error by setting the image access to public and read-only");
            this.s3ClientMock.VerifyAll();
        }

        /*===========================*** Tests for DeleteDeviceModelImageAsync() **===========================*/

        [Test]
        public async Task FailedDeletingDeviceModelImageShouldThrowsAnInternalServerError()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var bucketName = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);

            _ = this.s3ClientMock.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(""));

            // Act
            var act = async () => await this.awsDeviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelId);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>("Unable to delete the image from the blob storage.");
            this.s3ClientMock.VerifyAll();
        }

        [Test]
        public async Task SucessDeletingDeviceModelImageShouldNotThrowsAnInternalServerError()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var bucketName = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);

            _ = this.s3ClientMock.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteObjectResponse());

            // Act
            await this.awsDeviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelId);

            // Assert
            this.s3ClientMock.VerifyAll();
        }

        /*===========================*** Tests for SetDefaultImageToModel() **===========================*/

        [Test]
        public async Task SetDefaultImageToModeShouldUploadImageAndReturnAUri()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var bucketName = Fixture.Create<string>();
            var region = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns(region);
            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());


            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.s3ClientMock.Setup(s3 => s3.PutACLAsync(It.IsAny<PutACLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutACLResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            var expectedRetunUrl = $"https://{bucketName}.s3.{RegionEndpoint.GetBySystemName(region)}.amazonaws.com/{deviceModelId}";


            // Act
            var result = await this.awsDeviceModelImageManager.SetDefaultImageToModel(deviceModelId);

            // Assert
            Assert.NotNull(result);
            _ = result.Should().Be(expectedRetunUrl);
            this.s3ClientMock.VerifyAll();
        }

        [Test]
        public void SetDefaultImageToModeShouldThrowsAnExceptionForPutObjectResStatusCode()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var bucketName = "invalid bucket Name for example";
            var region = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns(region);
            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());


            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.BadGateway
                });


            // Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            {
                // Act
                _ = await this.awsDeviceModelImageManager.SetDefaultImageToModel(deviceModelId);

            }, "Error by uploading the image in S3 Storage");
            this.s3ClientMock.VerifyAll();
        }

        [Test]
        public void SetDefaultImageToModelShouldThrowsAnExceptionForPutACLStatusCode()
        {
            // Arrange
            var deviceModelId = Fixture.Create<string>();
            var bucketName = "invalid bucket Name for example";
            var region = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns(region);
            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());


            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });
            _ = this.s3ClientMock.Setup(s3 => s3.PutACLAsync(It.IsAny<PutACLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutACLResponse
                {
                    HttpStatusCode = HttpStatusCode.BadGateway
                });

            // Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            {
                // Act
                _ = await this.awsDeviceModelImageManager.SetDefaultImageToModel(deviceModelId);

            }, "Error by setting the image access to public and read-only");
            this.s3ClientMock.VerifyAll();
        }


        /*===========================*** Tests for InitializeDefaultImageBlob() **===========================*/

        [Test]
        public async Task InitializeDefaultImageBlobShouldUploadDefaultImage()
        {
            // Arrange
            var bucketName = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());

            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.s3ClientMock.Setup(s3 => s3.PutACLAsync(It.IsAny<PutACLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutACLResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });


            // Act
            await this.awsDeviceModelImageManager.InitializeDefaultImageBlob();

            // Assert
            this.s3ClientMock.VerifyAll();
        }

        [Test]
        public void InitializeDefaultImageBlobShouldThrowsAnExceptionForPutObjectResStatusCode()
        {
            // Arrange
            var bucketName = "invalid bucket Name for example";

            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());

            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.BadGateway
                });


            // Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            {
                // Act
                await this.awsDeviceModelImageManager.InitializeDefaultImageBlob();

            }, "Error by uploading the image in S3 Storage");
            this.s3ClientMock.VerifyAll();
        }

        [Test]
        public void InitializeDefaultImageBlobShouldThrowsAnExceptionForPutACLStatusCode()
        {
            // Arrange
            var bucketName = "invalid bucket Name for example";

            _ = this.mockConfigHandler.Setup(handler => handler.AWSBucketName).Returns(bucketName);
            _ = this.mockConfigHandler.Setup(handler => handler.StorageAccountDeviceModelImageMaxAge).Returns(Fixture.Create<int>());

            _ = this.s3ClientMock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });
            _ = this.s3ClientMock.Setup(s3 => s3.PutACLAsync(It.IsAny<PutACLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutACLResponse
                {
                    HttpStatusCode = HttpStatusCode.BadGateway
                });

            // Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            {
                // Act
                await this.awsDeviceModelImageManager.InitializeDefaultImageBlob();

            }, "Error by setting the image access to public and read-only");
            this.s3ClientMock.VerifyAll();
        }

        /*===========================*** Tests for SyncImagesCacheControl() **===========================*/


        [Test]
        public void SyncImagesCacheControlShouldThrowsANotImplmentedException()
        {
            // Arrange
            // We just verify that the method throw NotImplmentedExeception()

            // Act
            var act = () => this.awsDeviceModelImageManager.SyncImagesCacheControl();

            // Assert
            _ = act.Should().ThrowAsync<NotImplementedException>();
        }

        /*===========================*** Tests for ComputeImageUri() **===========================*/


        [Test]
        public void ComputeImageUriShouldThrowsANotImplmentedException()
        {
            // Arrange
            var thingTypelId = Fixture.Create<string>();

            // Assert
            _ = Assert.Throws<NotImplementedException>(() =>
            {
                // Act
                _ = this.awsDeviceModelImageManager.ComputeImageUri(thingTypelId);
            });
        }
    }
}
