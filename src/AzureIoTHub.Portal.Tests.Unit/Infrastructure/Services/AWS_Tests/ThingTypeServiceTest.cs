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
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities.AWS;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure.Services.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using Stream = System.IO.Stream;

    [TestFixture]
    public class ThingTypeServiceTest : BackendUnitTest
    {

        private Mock<IThingTypeRepository> mockThingTypeRepository;
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IAmazonIoT> amazonIotClient;
        private Mock<IDeviceModelImageManager> mockDeviceModelImageManager;


        private IThingTypeService thingTypeService;

        [SetUp]
        public void SetUp()
        {
            base.Setup();
            this.mockThingTypeRepository = MockRepository.Create<IThingTypeRepository>();
            this.mockUnitOfWork = MockRepository.Create<IUnitOfWork>();
            this.amazonIotClient = MockRepository.Create<IAmazonIoT>();
            this.mockDeviceModelImageManager = MockRepository.Create<IDeviceModelImageManager>();

            _ = ServiceCollection.AddSingleton(this.mockThingTypeRepository.Object);
            _ = ServiceCollection.AddSingleton(this.amazonIotClient.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockUnitOfWork.Object);
            _ = ServiceCollection.AddSingleton(this.mockDeviceModelImageManager.Object);
            _ = ServiceCollection.AddSingleton<IThingTypeService, ThingTypeService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.thingTypeService = Services.GetRequiredService<IThingTypeService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateAThingTypeShouldReturnAValue()
        {
            // Arrange
            var expectedAvatarUrl = Fixture.Create<string>();
            var ThingTypeID = Fixture.Create<string>();
            var thingDevice = new ThingTypeDto()
            {
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
                    ThingTypeId = ThingTypeID,
                    HttpStatusCode = HttpStatusCode.OK
                });


            _ = this.mockThingTypeRepository.Setup(repository => repository.InsertAndGetIdAsync(It.IsAny<ThingType>()))
                .Returns(Task.FromResult(ThingTypeID));

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.SetDefaultImageToModel(ThingTypeID))
                .ReturnsAsync(expectedAvatarUrl);

            //Act
            var result = await this.thingTypeService.CreateThingType(thingDevice);

            //Assert
            Assert.AreEqual(thingDevice.ThingTypeID, result);

            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateAThingTypeWithNullDescriptionTagsAndSearchableAttributeShouldReturnAValue()
        {
            // Arrange
            var ThingTypeID = Fixture.Create<string>();
            var expectedAvatarUrl = Fixture.Create<string>();

            var thingDevice = new ThingTypeDto()
            {
                ThingTypeName = Fixture.Create<string>(),
                ThingTypeDescription = null,
                Tags = null,
                ThingTypeSearchableAttDtos = null
            };

            _ = this.amazonIotClient.Setup(s3 => s3.CreateThingTypeAsync(It.IsAny<CreateThingTypeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateThingTypeResponse
                {
                    ThingTypeId = ThingTypeID,
                    HttpStatusCode = HttpStatusCode.OK
                });


            _ = this.mockThingTypeRepository.Setup(repository => repository.InsertAndGetIdAsync(It.IsAny<ThingType>()))
                .Returns(Task.FromResult(ThingTypeID));

            _ = this.mockUnitOfWork.Setup(work => work.SaveAsync())
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelImageManager.Setup(manager => manager.SetDefaultImageToModel(ThingTypeID))
                .ReturnsAsync(expectedAvatarUrl);
            //Act
            var result = await this.thingTypeService.CreateThingType(thingDevice);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(thingDevice.ThingTypeID, result);

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

        [Test]
        public async Task GetThingTypeAvatarShouldReturnThingTypeAvatar()
        {
            // Arrange
            var thingTypeDto = Fixture.Create<ThingTypeDto>();
            var expectedAvatarUrl = Fixture.Create<Uri>();
            _ = this.mockDeviceModelImageManager.Setup(manager => manager.ComputeImageUri(thingTypeDto.ThingTypeID))
                .Returns(expectedAvatarUrl);

            // Act
            var result = await this.thingTypeService.GetThingTypeAvatar(thingTypeDto.ThingTypeID);

            // Assert
            _ = result.Should().Be(expectedAvatarUrl.ToString());
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task UpdateThingTypeAvatarShouldUpdateThingTypeAvatar()
        {
            // Arrange
            var thingTypeDto = Fixture.Create<ThingTypeDto>();
            var expectedAvatarUrl = Fixture.Create<string>();

            var mockFormFile = MockRepository.Create<IFormFile>();

            _ = this.mockDeviceModelImageManager.Setup(manager =>
                    manager.ChangeDeviceModelImageAsync(thingTypeDto.ThingTypeID, It.IsAny<Stream>()))
                .ReturnsAsync(expectedAvatarUrl);

            _ = mockFormFile.Setup(file => file.OpenReadStream())
                .Returns(Stream.Null);

            // Act
            var result = await this.thingTypeService.UpdateThingTypeAvatar(thingTypeDto.ThingTypeID, mockFormFile.Object);

            // Assert
            _ = result.Should().Be(expectedAvatarUrl);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteThingTypeAvatarShouldDeleteThingTypeAvatar()
        {
            // Arrange
            var thingTypeDto = Fixture.Create<ThingTypeDto>();

            _ = this.mockDeviceModelImageManager
                .Setup(manager => manager.DeleteDeviceModelImageAsync(thingTypeDto.ThingTypeID))
                .Returns(Task.CompletedTask);

            // Act
            await this.thingTypeService.DeleteThingTypeAvatar(thingTypeDto.ThingTypeID);

            // Assert
            MockRepository.VerifyAll();
        }
    }
}
