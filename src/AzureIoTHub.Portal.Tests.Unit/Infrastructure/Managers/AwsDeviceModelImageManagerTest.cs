// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Managers
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using AutoFixture;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
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
                .ReturnsAsync(new PutObjectResponse());

            _ = this.s3ClientMock.Setup(s3 => s3.PutACLAsync(It.IsAny<PutACLRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutACLResponse());

            var expectedRetunUri = $"https://{bucketName}.s3.{RegionEndpoint.GetBySystemName(region)}.amazonaws.com/{deviceModelId}";


            // Act

            var result = await this.awsDeviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, imageAsMemoryStream);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(expectedRetunUri, result);
            this.s3ClientMock.VerifyAll();
        }

    }
}
