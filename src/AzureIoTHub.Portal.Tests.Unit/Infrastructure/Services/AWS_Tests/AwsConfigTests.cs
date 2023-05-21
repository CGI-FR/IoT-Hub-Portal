// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Services.AWS_Tests
{
    using Amazon.GreengrassV2;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Application.Services;
    using AutoMapper;
    using AzureIoTHub.Portal.Infrastructure.Services.AWS;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using System.Net;
    using Amazon.GreengrassV2.Model;
    using System.Threading;
    using AzureIoTHub.Portal.Models.v10;
    using AutoFixture;
    using System.Threading.Tasks;

    [TestFixture]
    public class AwsConfigTests : BackendUnitTest
    {
        private Mock<IAmazonGreengrassV2> mockGreengrasClient;
        private Mock<IDeviceModelImageManager> mocDeviceModelImageManager;
        private Mock<IEdgeDeviceModelRepository> mockEdgeModelRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;

        private IConfigService awsConfigService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();
            this.mockEdgeModelRepository = MockRepository.Create<IEdgeDeviceModelRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockGreengrasClient = MockRepository.Create<IAmazonGreengrassV2>();
            this.mocDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();

            _ = ServiceCollection.AddSingleton(this.mockEdgeModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockGreengrasClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mocDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton<IConfigService, AwsConfigService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.awsConfigService = Services.GetRequiredService<IConfigService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateDeploymentWithComponentsShouldCreateComponentsAndDeployment()
        {
            //Act
            var edge = Fixture.Create<IoTEdgeModel>();

            _ = this.mockGreengrasClient.Setup(s3 => s3.CreateDeploymentAsync(It.IsAny<CreateDeploymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateDeploymentResponse
                {
                    HttpStatusCode = HttpStatusCode.Created
                });
            _ = this.mockGreengrasClient.Setup(s3 => s3.CreateComponentVersionAsync(It.IsAny<CreateComponentVersionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateComponentVersionResponse
                {
                    HttpStatusCode = HttpStatusCode.Created
                });

            //Arrange
            await this.awsConfigService.RollOutEdgeModelConfiguration(edge);

            //Assert
            MockRepository.VerifyAll();

        }


    }
}
