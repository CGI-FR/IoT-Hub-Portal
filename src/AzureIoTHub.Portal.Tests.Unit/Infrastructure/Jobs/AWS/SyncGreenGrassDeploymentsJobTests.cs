// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Jobs.AWS
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using Amazon.IoT;
    using AutoFixture;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Jobs.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Quartz;

    public class SyncGreenGrassDeploymentsJobTests : BackendUnitTest
    {
        private IJob syncGreenGrassJob;

        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IEdgeDeviceModelRepository> mockEdgeDeviceModelRepository;
        private Mock<IAmazonIoT> mockAmazonIoTClient;
        private Mock<IAmazonGreengrassV2> mockAmazonGreenGrass;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockEdgeDeviceModelRepository = MockRepository.Create<IEdgeDeviceModelRepository>();
            this.mockAmazonIoTClient = MockRepository.Create<IAmazonIoT>();
            this.mockAmazonGreenGrass = MockRepository.Create<IAmazonGreengrassV2>();

            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockEdgeDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonIoTClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockAmazonGreenGrass.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncGreenGrassDeploymentsJob>();


            Services = ServiceCollection.BuildServiceProvider();

            this.syncGreenGrassJob = Services.GetRequiredService<IJob>();
        }

        [Test]
        public async Task ExecuteSyncExistingAWSDeploymentsAndCreateNinExistingDeploymentInDB()
        {

            //Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

            var deploymentId = Fixture.Create<string>();

            var listDeploymentsInAws = new ListDeploymentsResponse
            {
                Deployments = new List<Deployment>()
                {
                    new Deployment
                    {
                        DeploymentId = deploymentId,
                    },
                    new Deployment
                    {
                        DeploymentId = Fixture.Create<string>(),
                    },
                    new Deployment
                    {
                        DeploymentId = Fixture.Create<string>(),
                    }
                }
            };
            var existingDeployments = new List<EdgeDeviceModel>
            {
                new EdgeDeviceModel
                {
                    Id = Fixture.Create<string>(),
                    ExternalIdentifier = deploymentId,
                },
                new EdgeDeviceModel
                {
                    Id = Fixture.Create<string>(),
                    ExternalIdentifier = Fixture.Create<string>(),
                }
            };

            _ = this.mockAmazonGreenGrass.Setup(greengrass => greengrass.ListDeploymentsAsync(It.IsAny<ListDeploymentsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(listDeploymentsInAws);

            _ = this.mockEdgeDeviceModelRepository.Setup(u => u.GetAllAsync(null, It.IsAny<CancellationToken>()))
              .ReturnsAsync(existingDeployments);

            _ = this.mockEdgeDeviceModelRepository.Setup(u => u.InsertAsync(It.Is<EdgeDeviceModel>(s => !s.ExternalIdentifier.Equals(deploymentId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);
            _ = this.mockDeviceModelImageManager.Setup(c => c.SetDefaultImageToModel(It.Is<string>(s => !s.Equals(deploymentId, StringComparison.Ordinal))))
                .ReturnsAsync(Fixture.Create<string>());
            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();

        }
    }
}
