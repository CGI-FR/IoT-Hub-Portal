// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Services.AWS_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoFixture;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Services.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ThingTypeServiceTest : BackendUnitTest
    {

        private Mock<IThingTypeRepository> mockThingTypeRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IAmazonIoT> amazonIotClient;

        private IThingTypeService thingTypeService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();
            this.mockThingTypeRepository = MockRepository.Create<IThingTypeRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.amazonIotClient = MockRepository.Create<IAmazonIoT>();

            _ = ServiceCollection.AddSingleton(this.mockThingTypeRepository.Object);
            _ = ServiceCollection.AddSingleton(this.amazonIotClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton<IThingTypeService, ThingTypeService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.thingTypeService = Services.GetRequiredService<IThingTypeService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateAThingTypeShouldReturnAValue()
        {
            // Arrange

            var searchAttr1 = new ThingTypeSearchableAttDto()
            {
                Name = Fixture.Create<string>()
            };
            var searchAttr2 = new ThingTypeSearchableAttDto()
            {
                Name = Fixture.Create<string>()
            };

            var thingDevice = new ThingTypeDto()
            {
                ThingTypeID = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeDescription = Fixture.Create<string>(),
                Tags = new List<ThingTypeTagDto>(){
                    new ThingTypeTagDto() { Key = Fixture.Create<string>(), Value = Fixture.Create<string>()},
                    new ThingTypeTagDto() { Key = Fixture.Create<string>(), Value = Fixture.Create<string>()},

                },
                ThingTypeSearchableAttDtos = new List<ThingTypeSearchableAttDto>(){
                    new ThingTypeSearchableAttDto(){ Name = Fixture.Create<string>()},
                    new ThingTypeSearchableAttDto(){ Name = Fixture.Create<string>()},
                }
            };

            _ = this.amazonIotClient.Setup(s3 => s3.CreateThingTypeAsync(It.IsAny<CreateThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingTypeResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });


            _ = this.mockThingTypeRepository.Setup(repository => repository.InsertAsync(It.IsAny<ThingType>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            //Act
            var result = await this.thingTypeService.CreateThingType(thingDevice);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(thingDevice.ThingTypeID, result.ThingTypeID);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateAThingTypeWithNullDescriptionTagsAndSearchableAttributeShouldReturnAValue()
        {
            // Arrange


            var thingDevice = new ThingTypeDto()
            {
                ThingTypeID = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeDescription = null,
                Tags = null,
                ThingTypeSearchableAttDtos = null
            };

            _ = this.amazonIotClient.Setup(s3 => s3.CreateThingTypeAsync(It.IsAny<CreateThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingTypeResponse
                {
                    HttpStatusCode = HttpStatusCode.OK
                });


            _ = this.mockThingTypeRepository.Setup(repository => repository.InsertAsync(It.IsAny<ThingType>()))
                .Returns(Task.CompletedTask);

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            //Act
            var result = await this.thingTypeService.CreateThingType(thingDevice);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(thingDevice.ThingTypeID, result.ThingTypeID);

            MockRepository.VerifyAll();
        }
        [Test]
        public void CreateANullThingTypeShouldThrowNUllPointerException()
        {
            //Act
            _ = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                // Act
                _ = await this.thingTypeService.CreateThingType(null);


            }, "Null pointer Exception");

        }

        [Test]
        public void CreateThingTypeShouldThrowError500WhenCreateThigFails()
        {
            // Arrange

            var searchAttr1 = new ThingTypeSearchableAttDto()
            {
                Name = Fixture.Create<string>()
            };
            var searchAttr2 = new ThingTypeSearchableAttDto()
            {
                Name = Fixture.Create<string>()
            };

            var thingDevice = new ThingTypeDto()
            {
                ThingTypeID = Fixture.Create<string>(),
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeDescription = Fixture.Create<string>(),
                Tags = new List<ThingTypeTagDto>(){
                    new ThingTypeTagDto() { Key = Fixture.Create<string>(), Value = Fixture.Create<string>()},
                    new ThingTypeTagDto() { Key = Fixture.Create<string>(), Value = Fixture.Create<string>()},

                },
                ThingTypeSearchableAttDtos = new List<ThingTypeSearchableAttDto>(){
                    new ThingTypeSearchableAttDto(){ Name = Fixture.Create<string>()},
                    new ThingTypeSearchableAttDto(){ Name = Fixture.Create<string>()},
                }
            };

            _ = this.amazonIotClient.Setup(s3 => s3.CreateThingTypeAsync(It.IsAny<CreateThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingTypeResponse
                {
                    HttpStatusCode = HttpStatusCode.BadGateway
                });

            //Assert
            _ = Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            {
                // Act
                _ = await this.thingTypeService.CreateThingType(thingDevice);


            }, "Null pointer Exception");

            MockRepository.VerifyAll();
        }
    }
}
