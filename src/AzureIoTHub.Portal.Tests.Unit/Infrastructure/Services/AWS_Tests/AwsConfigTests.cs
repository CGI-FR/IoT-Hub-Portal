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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json.Linq;
    using System.Linq;

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
            // Arrange
            var deploymentId = Fixture.Create<string>();

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns("eu-west-1");
            _ = this.mockConfigHandler.Setup(handler => handler.AWSAccountId).Returns("00000000");

            var edge = Fixture.Create<IoTEdgeModel>();
            // Simulate a custom/private component
            edge.EdgeModules.First().Id = string.Empty;

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
                    HttpStatusCode = HttpStatusCode.Created,
                    DeploymentId = deploymentId
                });

            _ = this.mockGreengrasClient.Setup(s3 => s3.DescribeComponentAsync(It.IsAny<DescribeComponentRequest>(), It.IsAny<CancellationToken>()))

                .ThrowsAsync(new Amazon.GreengrassV2.Model.ResourceNotFoundException("Resource Not found"));
            _ = this.mockGreengrasClient.Setup(s3 => s3.CreateComponentVersionAsync(It.IsAny<CreateComponentVersionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateComponentVersionResponse
                {
                    HttpStatusCode = HttpStatusCode.Created
                });

            // Act
            var result = _ = await this.awsConfigService.RollOutEdgeModelConfiguration(edge);

            // Assert
            Assert.AreEqual(deploymentId, result);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeploymentWithExistingComponentsAndExistingThingGroupAndThingTypeShouldCreateTheDeployment()
        {
            // Arrange
            var deploymentId = Fixture.Create<string>();
            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns("eu-west-1");
            _ = this.mockConfigHandler.Setup(handler => handler.AWSAccountId).Returns("00000000");

            var edge = Fixture.Create<IoTEdgeModel>();
            // Simulate a custom/private component
            edge.EdgeModules.First().Id = string.Empty;

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
                    HttpStatusCode = HttpStatusCode.Created,
                    DeploymentId = deploymentId
                });

            _ = this.mockGreengrasClient.Setup(s3 => s3.DescribeComponentAsync(It.IsAny<DescribeComponentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DescribeComponentResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await this.awsConfigService.RollOutEdgeModelConfiguration(edge);

            // Assert
            Assert.AreEqual(deploymentId, result);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeploymentWithNonExistingComponentsAndNonExistingThingGroupAndThingTypeShouldCreateThingGroupAndThingTypeAndTheDeployment()
        {
            // Arrange
            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns("eu-west-1");
            _ = this.mockConfigHandler.Setup(handler => handler.AWSAccountId).Returns("00000000");

            var edge = Fixture.Create<IoTEdgeModel>();
            // Simulate a custom/private component
            edge.EdgeModules.First().Id = string.Empty;

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
            _ = this.mockGreengrasClient.Setup(s3 => s3.DescribeComponentAsync(It.IsAny<DescribeComponentRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Amazon.GreengrassV2.Model.ResourceNotFoundException("Resource Not found"));

            _ = this.mockGreengrasClient.Setup(s3 => s3.CreateComponentVersionAsync(It.IsAny<CreateComponentVersionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateComponentVersionResponse
                {
                    HttpStatusCode = HttpStatusCode.Created
                });

            // Act
            _ = await this.awsConfigService.RollOutEdgeModelConfiguration(edge);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task GetAllDeploymentComponentsShouldRetreiveImageUriAndEnvironmentVariables()
        {
            //Act
            var edge = Fixture.Create<IoTEdgeModel>();
            using var recipeAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Fixture.Create<JObject>().ToString()));

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns("eu-west-1");
            _ = this.mockConfigHandler.Setup(handler => handler.AWSAccountId).Returns("00000000");

            _ = this.mockGreengrasClient.Setup(s3 => s3.GetDeploymentAsync(It.IsAny<GetDeploymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetDeploymentResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    Components = new Dictionary<string, ComponentDeploymentSpecification>
                    {
                        {"test", new ComponentDeploymentSpecification()}

                    }
                });

            _ = this.mockGreengrasClient.Setup(s3 => s3.GetComponentAsync(It.IsAny<GetComponentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetComponentResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    Recipe = recipeAsMemoryStream,
                    RecipeOutputFormat = RecipeOutputFormat.JSON
                });

            //Arrange
            _ = await this.awsConfigService.GetConfigModuleList(edge.ExternalIdentifier);

            //Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeploymentShouldDeleteTheDeploymentVersionAndAllItsComponentsVersions()
        {
            //Act
            var edge = Fixture.Create<IoTEdgeModel>();
            using var recipeAsMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Fixture.Create<JObject>().ToString()));

            _ = this.mockConfigHandler.Setup(handler => handler.AWSRegion).Returns("eu-west-1");
            _ = this.mockConfigHandler.Setup(handler => handler.AWSAccountId).Returns("00000000");

            _ = this.mockGreengrasClient.Setup(s3 => s3.GetDeploymentAsync(It.IsAny<GetDeploymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetDeploymentResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    Components = new Dictionary<string, ComponentDeploymentSpecification>
                    {
                        {"test", new ComponentDeploymentSpecification()}

                    }
                });

            _ = this.mockGreengrasClient.Setup(s3 => s3.GetComponentAsync(It.IsAny<GetComponentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetComponentResponse
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    Recipe = recipeAsMemoryStream,
                    RecipeOutputFormat = RecipeOutputFormat.JSON
                });

            _ = this.mockGreengrasClient.Setup(s3 => s3.DeleteComponentAsync(It.IsAny<DeleteComponentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteComponentResponse
                {
                    HttpStatusCode = HttpStatusCode.NoContent
                });

            _ = this.mockGreengrasClient.Setup(s3 => s3.CancelDeploymentAsync(It.IsAny<CancelDeploymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CancelDeploymentResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });

            _ = this.mockGreengrasClient.Setup(s3 => s3.DeleteDeploymentAsync(It.IsAny<DeleteDeploymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteDeploymentResponse
                {
                    HttpStatusCode = HttpStatusCode.NoContent
                });
            //Arrange
            await this.awsConfigService.DeleteConfiguration(edge.ExternalIdentifier);

            //Assert
            MockRepository.VerifyAll();

        }
    }
}
