// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Infrastructure.Jobs.AWS
{
    public class SyncThingTypesJobTests : BackendUnitTest
    {
        private IJob syncThingTypesJob;

        private Mock<IAmazonIoT> iaAmazon;
        private Mock<IDeviceModelImageManager> mockAWSImageManager;
        private Mock<IExternalDeviceService> mockExternalDeviceService;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IDeviceModelRepository> mockDeviceModelRepository;

        public override void Setup()
        {
            base.Setup();

            this.mockAWSImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockExternalDeviceService = MockRepository.Create<IExternalDeviceService>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.mockDeviceModelRepository = MockRepository.Create<IDeviceModelRepository>();
            this.iaAmazon = MockRepository.Create<IAmazonIoT>();

            _ = ServiceCollection.AddSingleton(this.mockAWSImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockExternalDeviceService.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelRepository.Object);
            _ = ServiceCollection.AddSingleton(this.iaAmazon.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncThingTypesJob>();


            Services = ServiceCollection.BuildServiceProvider();

            this.syncThingTypesJob = Services.GetRequiredService<IJob>();
        }


        [Test]
        public async Task Execute_SyncNewAndExistingAndDepprecatedThingTypes_DeviceModelsSynced()
        {
            // Arrange
            var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();


            var existingDeviceModelName = Fixture.Create<string>();
            var newDeviceModelName = Fixture.Create<string>();
            var depcrecatedDeviceModelName = Fixture.Create<string>();

            var thingTypesListing = new ListThingTypesResponse
            {
                ThingTypes = new List<ThingTypeDefinition>()
                {
                    new ThingTypeDefinition
                    {
                        ThingTypeName = newDeviceModelName,
                    },
                    new ThingTypeDefinition
                    {
                        ThingTypeName = existingDeviceModelName,
                    },
                    new ThingTypeDefinition
                    {
                        ThingTypeName = depcrecatedDeviceModelName,
                    }
                }
            };

            _ = this.iaAmazon.Setup(client => client.ListThingTypesAsync(It.IsAny<ListThingTypesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(thingTypesListing);

            var existingThingType = new DescribeThingTypeResponse
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = existingDeviceModelName,
                ThingTypeName = existingDeviceModelName,
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            var newThingType = new DescribeThingTypeResponse
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = newDeviceModelName,
                ThingTypeName = newDeviceModelName,
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            var depcrecatedThingType = new DescribeThingTypeResponse
            {
                ThingTypeArn = Fixture.Create<string>(),
                ThingTypeId = depcrecatedDeviceModelName,
                ThingTypeName = depcrecatedDeviceModelName,
                ThingTypeMetadata = new ThingTypeMetadata
                {
                    Deprecated = true,
                    DeprecationDate = new DateTime(2023, 6, 11, 10, 30, 0)
                }
            };



            _ = this.iaAmazon.Setup(client => client.DescribeThingTypeAsync(It.Is<DescribeThingTypeRequest>(c => c.ThingTypeName == existingThingType.ThingTypeName), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingThingType);
            _ = this.iaAmazon.Setup(client => client.DescribeThingTypeAsync(It.Is<DescribeThingTypeRequest>(c => c.ThingTypeName == newThingType.ThingTypeName), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newThingType);
            _ = this.iaAmazon.Setup(client => client.DescribeThingTypeAsync(It.Is<DescribeThingTypeRequest>(c => c.ThingTypeName == depcrecatedThingType.ThingTypeName), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(depcrecatedThingType);
            _ = this.iaAmazon.Setup(client => client.DeleteThingTypeAsync(It.Is<DeleteThingTypeRequest>(c => c.ThingTypeName == depcrecatedThingType.ThingTypeName), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Fixture.Create<DeleteThingTypeResponse>);
            _ = this.iaAmazon.Setup(client => client.DeleteDynamicThingGroupAsync(It.Is<DeleteDynamicThingGroupRequest>(c => c.ThingGroupName == depcrecatedThingType.ThingTypeName), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Fixture.Create<DeleteDynamicThingGroupResponse>);

            _ = this.mockExternalDeviceService.Setup(client => client.IsEdgeDeviceModel(It.IsAny<ExternalDeviceModelDto>()))
                .ReturnsAsync(false);

            var existingDeviceModel = new DeviceModel
            {
                Id = existingDeviceModelName,
                Name = existingDeviceModelName,
            };
            var depcrecatedDeviceModel = new DeviceModel
            {
                Id = depcrecatedDeviceModelName,
                Name = depcrecatedDeviceModelName,
            };

            var existingDeviceModels = new List<DeviceModel>
            {
                existingDeviceModel,
                depcrecatedDeviceModel
            };

            _ = this.mockDeviceModelRepository.Setup(u => u.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingDeviceModels);

            _ = this.mockDeviceModelRepository.Setup(u => u.GetByIdAsync(It.Is<string>(s => s.Equals(existingDeviceModel.Id, StringComparison.Ordinal)), It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync(existingDeviceModel);
            _ = this.mockDeviceModelRepository.Setup(u => u.GetByIdAsync(It.Is<string>(s => !s.Equals(existingDeviceModel.Id, StringComparison.Ordinal)), It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync(default(DeviceModel));

            _ = this.mockDeviceModelRepository.Setup(u => u.InsertAsync(It.Is<DeviceModel>(s => s.Name.Equals(newDeviceModelName, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            this.mockDeviceModelRepository.Setup(u => u.Update(It.IsAny<DeviceModel>()))
                .Verifiable();
            this.mockDeviceModelRepository.Setup(u => u.Delete(It.Is<string>(s => s.Equals(depcrecatedDeviceModel.Id, StringComparison.Ordinal))))
                .Verifiable();

            _ = this.mockAWSImageManager.Setup(c => c.SetDefaultImageToModel(It.Is<string>(s => s.Equals(newDeviceModelName, StringComparison.Ordinal))))
                .ReturnsAsync(Fixture.Create<string>());
            _ = this.mockAWSImageManager.Setup(c => c.DeleteDeviceModelImageAsync(It.Is<string>(s => s.Equals(depcrecatedDeviceModel.Id, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(c => c.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await this.syncThingTypesJob.Execute(mockJobExecutionContext.Object);

            // Assert
            MockRepository.VerifyAll();
        }
    }

}
