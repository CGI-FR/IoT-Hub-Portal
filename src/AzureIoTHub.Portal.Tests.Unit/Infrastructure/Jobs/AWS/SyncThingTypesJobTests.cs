// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Jobs.AWS
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoFixture;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Infrastructure.Jobs.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Quartz;

    public class SyncThingTypesJobTests : BackendUnitTest
    {
        private IJob syncThingTypesJob;

        private Mock<IAmazonIoT> iaAmazon;
        private Mock<IDeviceModelImageManager> mockAWSImageManager;
        private Mock<IUnitOfWork> mockUnitOfWork;

        public override void Setup()
        {
            base.Setup();

            this.mockAWSImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.iaAmazon = MockRepository.Create<IAmazonIoT>();

            _ = ServiceCollection.AddSingleton(this.mockAWSImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
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
                ThingTypeId = existingDeviceModelName,
                ThingTypeName = existingDeviceModelName,
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            var newThingType = new DescribeThingTypeResponse
            {
                ThingTypeId = newDeviceModelName,
                ThingTypeName = newDeviceModelName,
                ThingTypeMetadata = new ThingTypeMetadata()
            };

            var depcrecatedThingType = new DescribeThingTypeResponse
            {
                ThingTypeId = depcrecatedDeviceModelName,
                ThingTypeName = depcrecatedDeviceModelName,
                ThingTypeMetadata = new ThingTypeMetadata
                {
                    Deprecated = true,
                }
            };

            _ = this.iaAmazon.Setup(client => client.DescribeThingTypeAsync(It.Is<DescribeThingTypeRequest>(c => c.ThingTypeName == existingThingType.ThingTypeName), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existingThingType);
            _ = this.iaAmazon.Setup(client => client.DescribeThingTypeAsync(It.Is<DescribeThingTypeRequest>(c => c.ThingTypeName == newThingType.ThingTypeName), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(newThingType);
            _ = this.iaAmazon.Setup(client => client.DescribeThingTypeAsync(It.Is<DescribeThingTypeRequest>(c => c.ThingTypeName == depcrecatedThingType.ThingTypeName), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(depcrecatedThingType);

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

            _ = this.mockUnitOfWork.Setup(u => u.DeviceModelRepository.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingDeviceModels);

            _ = this.mockUnitOfWork.Setup(u => u.DeviceModelRepository.GetByIdAsync(It.Is<string>(s => s.Equals(existingDeviceModel.Id, StringComparison.Ordinal)), It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync(existingDeviceModel);
            _ = this.mockUnitOfWork.Setup(u => u.DeviceModelRepository.GetByIdAsync(It.Is<string>(s => !s.Equals(existingDeviceModel.Id, StringComparison.Ordinal)), It.IsAny<Expression<Func<DeviceModel, object>>[]>()))
                .ReturnsAsync((DeviceModel)null);

            _ = this.mockUnitOfWork.Setup(u => u.DeviceModelRepository.InsertAsync(It.Is<DeviceModel>(s => s.Name.Equals(newDeviceModelName, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            this.mockUnitOfWork.Setup(u => u.DeviceModelRepository.Update(It.IsAny<DeviceModel>()))
                .Verifiable();
            this.mockUnitOfWork.Setup(u => u.DeviceModelRepository.Delete(It.Is<string>(s => s.Equals(depcrecatedDeviceModel.Id, StringComparison.Ordinal))))
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
