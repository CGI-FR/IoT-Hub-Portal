// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AutoFixture;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Shared;
    using AzureIoTHub.Portal.Infrastructure.Services;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AwsExternalDeviceServiceTests : BackendUnitTest
    {
        private Mock<IAmazonIoT> mockAmazonIot;

        private IExternalDeviceService externalDeviceModelService;

        public override void Setup()
        {
            base.Setup();

            this.mockAmazonIot = MockRepository.Create<IAmazonIoT>();

            _ = ServiceCollection.AddSingleton(this.mockAmazonIot.Object);
            _ = ServiceCollection.AddSingleton<IExternalDeviceService, AwsExternalDeviceService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.externalDeviceModelService = Services.GetRequiredService<IExternalDeviceService>();
            Mapper = Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CreateDeviceModel_NonExistingThingType_ThingTypeCreated()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            var createThingTypeResponse = new CreateThingTypeResponse
            {
                ThingTypeId = Fixture.Create<string>(),
                ThingTypeName = externalDeviceModelDto.Name
            };

            _ = this.mockAmazonIot.Setup(e => e.CreateThingTypeAsync(It.Is<CreateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createThingTypeResponse);

            _ = this.mockAmazonIot.Setup(c => c.DescribeThingGroupAsync(It.Is<DescribeThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ResourceNotFoundException(It.IsAny<string>()));

            _ = this.mockAmazonIot.Setup(c => c.CreateDynamicThingGroupAsync(It.Is<CreateDynamicThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)
                                                                                                                        && d.QueryString.Equals($"thingTypeName: {externalDeviceModelDto.Name}", StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new CreateDynamicThingGroupResponse());

            // Act
            var result = await this.externalDeviceModelService.CreateDeviceModel(externalDeviceModelDto);

            // Assert
            _ = result.Id.Should().Be(createThingTypeResponse.ThingTypeId);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateDeviceModel_ExistingThingType_ResourceAlreadyExistsExceptionIsThrown()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.CreateThingTypeAsync(It.Is<CreateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceAlreadyExistsException(Fixture.Create<string>()));

            // Act
            var act = () => this.externalDeviceModelService.CreateDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<Portal.Domain.Exceptions.ResourceAlreadyExistsException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_ExistingThingType_ThingTypeDepcrecated()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.DeprecateThingTypeAsync(It.Is<DeprecateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase) && !c.UndoDeprecate), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeprecateThingTypeResponse());

            _ = this.mockAmazonIot.Setup(c => c.DeleteDynamicThingGroupAsync(It.Is<DeleteDynamicThingGroupRequest>(d => d.ThingGroupName.Equals(externalDeviceModelDto.Name, StringComparison.OrdinalIgnoreCase)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new DeleteDynamicThingGroupResponse());

            // Act
            await this.externalDeviceModelService.DeleteDeviceModel(externalDeviceModelDto);

            // Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteDeviceModel_NonExistingThingType_ResourceNotFoundExceptionIsThrown()
        {
            // Arrange
            var externalDeviceModelDto = Fixture.Create<ExternalDeviceModelDto>();

            _ = this.mockAmazonIot.Setup(e => e.DeprecateThingTypeAsync(It.Is<DeprecateThingTypeRequest>(c => externalDeviceModelDto.Name.Equals(c.ThingTypeName, StringComparison.OrdinalIgnoreCase) && !c.UndoDeprecate), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceNotFoundException(Fixture.Create<string>()));

            // Act
            var act = () => this.externalDeviceModelService.DeleteDeviceModel(externalDeviceModelDto);

            // Assert
            _ = await act.Should().ThrowAsync<Portal.Domain.Exceptions.ResourceNotFoundException>();
            MockRepository.VerifyAll();
        }
    }
}
