// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Jobs
{
    using AutoFixture;
    using System;
    using Azure.Storage.Blobs;
    using AzureIoTHub.Portal.Server.Jobs;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Quartz;
    using Azure.Storage.Blobs.Models;
    using Azure;
    using System.Collections.Generic;
    using System.Threading;

    public class GetBaseImageUriFolderJobTest : BackendUnitTest
    {
        private IJob getBaseImageUriFolderJob;

        private Mock<BlobServiceClient> mockBlobServiceClient;
        private Mock<EnvVariableRegistry> mockEnvVariableRegistry;
        private Mock<BlobContainerClient> mockBlobContainerClient;

        public override void Setup()
        {
            base.Setup();

            this.mockBlobServiceClient = MockRepository.Create<BlobServiceClient>();
            this.mockEnvVariableRegistry = MockRepository.Create<EnvVariableRegistry>();
            this.mockBlobContainerClient = MockRepository.Create<BlobContainerClient>();

            _ = ServiceCollection.AddSingleton(this.mockBlobServiceClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockEnvVariableRegistry.Object);
            _ = ServiceCollection.AddSingleton<IJob, GetBaseImageUriFolderJob>();

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

            this.getBaseImageUriFolderJob = Services.GetRequiredService<IJob>();
        }

        [Test]
        public void ExecuteShouldGetBaseImageUriFolder()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var mockBlobContainer = MockRepository.Create<BlobContainerClient>();
            var expectedImageUri = Fixture.Create<Uri>();

            //_ = this.mockEnvVariableRegistry.Setup(x => x.BaseImageFolderUri).Returns(expectedImageUri);

            _ = this.mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockBlobContainer.Object);

            _ = mockBlobContainer.Setup(x => x.Uri).Returns(expectedImageUri);

            // Act
            _ = this.getBaseImageUriFolderJob.Execute(mockJobExecutionContext.Object);

            // Assert
            Assert.AreEqual(expectedImageUri, this.mockEnvVariableRegistry.Object.BaseImageFolderUri);
            MockRepository.VerifyAll();
        }
    }
}
