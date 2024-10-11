// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs.AWS
{
    

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
            var nonExistingDeploymentId = Fixture.Create<string>();

            var listDeploymentsInAws = new ListDeploymentsResponse
            {
                Deployments = new List<Deployment>()
                {
                    new Deployment
                    {
                        DeploymentId = deploymentId,
                        TargetArn = "arn:aws:iot:eu-west-1:0000000000:thinggroup/DemoEdgeModel"
                    },
                    new Deployment
                    {
                        DeploymentId = Fixture.Create<string>(),
                        TargetArn = "arn:aws:iot:eu-west-1:0000000000:thinggroup/toto"

                    },
                    new Deployment
                    {
                        DeploymentId = Fixture.Create<string>(),
                        TargetArn = "arn:aws:iot:eu-west-1:0000000000:thinggroup/titi"

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
                    Id = nonExistingDeploymentId,
                    ExternalIdentifier = nonExistingDeploymentId,
                }
            };

            _ = this.mockAmazonGreenGrass.Setup(greengrass => greengrass.ListDeploymentsAsync(It.IsAny<ListDeploymentsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(listDeploymentsInAws);

            _ = this.mockAmazonIoTClient.Setup(iot => iot.DescribeThingGroupAsync(It.IsAny<DescribeThingGroupRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Fixture.Create<DescribeThingGroupResponse>);

            _ = this.mockEdgeDeviceModelRepository.Setup(u => u.GetAllAsync(null, It.IsAny<CancellationToken>()))
              .ReturnsAsync(existingDeployments);

            _ = this.mockEdgeDeviceModelRepository.Setup(u => u.InsertAsync(It.Is<EdgeDeviceModel>(s => !s.ExternalIdentifier.Equals(deploymentId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);
            _ = this.mockDeviceModelImageManager.Setup(c => c.SetDefaultImageToModel(It.Is<string>(s => !s.Equals(deploymentId, StringComparison.Ordinal))))
                .ReturnsAsync(Fixture.Create<string>());

            this.mockEdgeDeviceModelRepository.Setup(u => u.Delete(It.Is<string>(s => s.Equals(nonExistingDeploymentId, StringComparison.Ordinal))))
               .Verifiable();

            _ = this.mockDeviceModelImageManager.Setup(c => c.DeleteDeviceModelImageAsync(It.Is<string>(s => s.Equals(nonExistingDeploymentId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncGreenGrassJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();

        }

    }
}
