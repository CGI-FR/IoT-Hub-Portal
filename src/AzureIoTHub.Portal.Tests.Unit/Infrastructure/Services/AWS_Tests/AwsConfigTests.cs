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
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AzureIoTHub.Portal.Domain.Entities;

    [TestFixture]
    public class AwsConfigTests : BackendUnitTest
    {
        private Mock<IAmazonGreengrassV2> mockGreengrasClient;
        private Mock<IAmazonIoT> mockIotClient;
        private Mock<IDeviceModelImageManager> mocDeviceModelImageManager;
        private Mock<IEdgeDeviceModelRepository> mockEdgeModelRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<ConfigHandler> mockConfigHandler;


        private IConfigService awsConfigService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();
            this.mockEdgeModelRepository = MockRepository.Create<IEdgeDeviceModelRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockGreengrasClient = MockRepository.Create<IAmazonGreengrassV2>();
            this.mockIotClient = MockRepository.Create<IAmazonIoT>();
            this.mocDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockConfigHandler = MockRepository.Create<ConfigHandler>();


            _ = ServiceCollection.AddSingleton(this.mockEdgeModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockGreengrasClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockIotClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mocDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton<IConfigService, AwsConfigService>();
            _ = ServiceCollection.AddSingleton(this.mockConfigHandler.Object);


            Services = ServiceCollection.BuildServiceProvider();

            this.awsConfigService = Services.GetRequiredService<IConfigService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateDeploymentWithComponentsAndExistingThingGroupAndThingTypeShouldCreateTheDeployment()
        {
            //Act

            var edge = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edge);

            _ = this.mockIotClient.Setup(s3 => s3.DescribeThingGroupAsync(It.IsAny<DescribeThingGroupRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingGroupResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockIotClient.Setup(s3 => s3.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeThingTypeResponse());

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

            _ = this.mockEdgeModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(edgeDeviceModelEntity);

            _ = this.mockEdgeModelRepository.Setup(repository => repository.Update(It.IsAny<EdgeDeviceModel>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            //Arrange
            await this.awsConfigService.RollOutEdgeModelConfiguration(edge);

            //Assert
            MockRepository.VerifyAll();

        }

        [Test]
        public async Task CreateDeploymentWithComponentsAndNonExistingThingGroupAndThingTypeShouldCreateThingGroupAndThingTypeAndTheDeployment()
        {
            //Act
            var edge = Fixture.Create<IoTEdgeModel>();
            var edgeDeviceModelEntity = Mapper.Map<EdgeDeviceModel>(edge);

            _ = this.mockIotClient.Setup(s3 => s3.DescribeThingGroupAsync(It.IsAny<DescribeThingGroupRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException("Resource Not found"));

            _ = this.mockIotClient.Setup(s3 => s3.CreateDynamicThingGroupAsync(It.IsAny<CreateDynamicThingGroupRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateDynamicThingGroupResponse());

            _ = this.mockIotClient.Setup(s3 => s3.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.IoT.Model.ResourceNotFoundException("Resource Not found"));

            _ = this.mockIotClient.Setup(s3 => s3.CreateThingTypeAsync(It.IsAny<CreateThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingTypeResponse());

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

            _ = this.mockEdgeModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(edgeDeviceModelEntity);

            _ = this.mockEdgeModelRepository.Setup(repository => repository.Update(It.IsAny<EdgeDeviceModel>()));
            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            //Arrange
            await this.awsConfigService.RollOutEdgeModelConfiguration(edge);

            //Assert
            MockRepository.VerifyAll();



        }
    }
}
