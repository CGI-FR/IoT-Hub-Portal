// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Jobs.AWS
{
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Moq;
    using Quartz;
    using Amazon.IoT;
    using AzureIoTHub.Portal.Domain.Repositories.AWS;
    using AzureIoTHub.Portal.Application.Managers;
    using Microsoft.Extensions.DependencyInjection;
    //using System.Threading.Tasks;
    //using Amazon.IoT.Model;
    //using System.Net;
    //using System.Threading;
    //using AzureIoTHub.Portal.Models.v10.AWS;
    //using AutoFixture;
    //using AzureIoTHub.Portal.Domain.Entities.AWS;
    //using System.Collections.Generic;
    //using System.Linq.Expressions;
    //using System;
    //using NUnit.Framework;
    using AzureIoTHub.Portal.Infrastructure.Jobs.AWS;

    public class SyncThingTypesJobTests : BackendUnitTest
    {
        private IJob syncThingTypesJob;

        private Mock<IAmazonIoT> iaAmazon;
        private Mock<IThingTypeRepository> mockThingTypeRepository;
        private Mock<IThingTypeSearchableAttRepository> mockThingTypeSearchAttrRepository;
        private Mock<IThingTypeTagRepository> mockThingTypeTagRepository;
        private Mock<IDeviceModelImageManager> mockAWSImageManager;
        private Mock<IUnitOfWork> mockUnitOfWork;

        public override void Setup()
        {
            base.Setup();

            this.mockThingTypeRepository = MockRepository.Create<IThingTypeRepository>();
            this.mockThingTypeSearchAttrRepository = MockRepository.Create<IThingTypeSearchableAttRepository>();
            this.mockThingTypeTagRepository = MockRepository.Create<IThingTypeTagRepository>();
            this.mockAWSImageManager = MockRepository.Create<IDeviceModelImageManager>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.iaAmazon = MockRepository.Create<IAmazonIoT>();

            _ = ServiceCollection.AddSingleton(this.mockThingTypeRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockThingTypeSearchAttrRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockThingTypeTagRepository.Object);
            _ = ServiceCollection.AddSingleton(this.mockAWSImageManager.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.iaAmazon.Object);
            _ = ServiceCollection.AddSingleton<IJob, SyncThingTypesJob>();


            Services = ServiceCollection.BuildServiceProvider();

            this.syncThingTypesJob = Services.GetRequiredService<IJob>();
        }

        /*
         [Test]
         public async Task ShouldListAllAWSThingTypes()
         {
             // Arrange
             var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();

             var searchableAttr = Fixture.CreateMany<ThingTypeSearchableAttDto>();

             var tag = Fixture.CreateMany<ThingTypeTagDto>();

             var thingTypeId = Fixture.Create<string>();
             var thingTypeName = Fixture.Create<string>();
             var thingTypeProperties = Fixture.Create<ThingTypeProperties>();
             var thingTypeMetadata = Fixture.Create<ThingTypeMetadata>();
             var response = Fixture.Create<ListThingTypesResponse>();
             var describeResponse = Fixture.Create<DescribeThingTypeResponse>();

             _ = this.iaAmazon.Setup(s3 => s3.ListThingTypesAsync(It.IsAny<ListThingTypesRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(response);
             foreach (var thingType in response.ThingTypes)
             {
                 _ = this.iaAmazon.Setup(s3 => s3.DescribeThingTypeAsync(It.IsAny<DescribeThingTypeRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(describeResponse);
             }

             // Act
             await this.syncThingTypesJob.Execute(mockJobExecutionContext.Object);

             // Assert
             MockRepository.VerifyAll();
         }

         [Test]
         public async Task ExecuteNewThingTypeShouldCreateNewThingType()
         {
             // Arrange
             var mockJobExecutionContext = MockRepository.Create<IJobExecutionContext>();
             var searchableAttr = new List<ThingTypeSearchableAttDto>
             {
                 new ThingTypeSearchableAttDto{Name = "search1"},
                 new ThingTypeSearchableAttDto{Name = "search2"},
             };

             var tags = new List<ThingTypeTagDto>
             {
                 new ThingTypeTagDto{Key = "key1", Value = "val1"},
                 new ThingTypeTagDto{Key = "key2", Value = "val2"}
             };

             var expectedThingType = new ThingTypeDto
             {
                 ThingTypeID = Fixture.Create<string>(),
                 ThingTypeName = Fixture.Create<string>(),
                 ThingTypeDescription = Fixture.Create<string>(),
                 ThingTypeSearchableAttDtos = searchableAttr,
                 Tags = tags
             };

             var thingType = Fixture.Create<ThingType>();


             _ = this.mockThingTypeRepository.Setup(repository => repository.GetByIdAsync(expectedThingType.ThingTypeID, d => d.Tags, d => d.ThingTypeSearchableAttributes))
                 .ReturnsAsync(thingType);

             _ = this.mockThingTypeRepository.Setup(repository => repository.InsertAsync(It.IsAny<ThingType>()))
                 .Returns(Task.CompletedTask);

             _ = this.mockThingTypeRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<ThingType, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<ThingType>
                 {
                     new ThingType
                     {
                         Id = expectedThingType.ThingTypeID
                     },
                     new ThingType
                     {
                         Id = Guid.NewGuid().ToString()
                     }
                 });

             this.mockThingTypeRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();
             this.mockThingTypeSearchAttrRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();
             this.mockThingTypeTagRepository.Setup(x => x.Delete(It.IsAny<string>())).Verifiable();

             _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                 .Returns(Task.CompletedTask);

             // Act
             await this.syncThingTypesJob.Execute(mockJobExecutionContext.Object);

             // Assert
             MockRepository.VerifyAll();

         }
        */
    }

}
