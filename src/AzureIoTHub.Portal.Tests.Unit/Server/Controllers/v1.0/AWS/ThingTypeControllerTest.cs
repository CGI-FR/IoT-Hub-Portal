// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Server.Controllers.v1._0.AWS
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Application.Services.AWS;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Server.Controllers.v1._0.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;
    using FluentAssertions;
    using AutoFixture;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Mvc.Routing;
    using System.Linq;

    [TestFixture]
    public class ThingTypeControllerTest : BackendUnitTest
    {
        private MockRepository mockRepository;

        private Mock<IThingTypeService> mockThingTypeService;
        private Mock<IThingTypeRepository> mockThingTypeRepository;
        private Mock<IUrlHelper> mockUrlHelper;

        [SetUp]
        public void SetUp()
        {

            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockThingTypeService = this.mockRepository.Create<IThingTypeService>();
            this.mockUrlHelper = this.mockRepository.Create<IUrlHelper>();
            this.mockThingTypeRepository = this.mockRepository.Create<IThingTypeRepository>();
        }

        [Test]
        public async Task GetListGreaterThan10ShouldReturnAListAndNextPage()
        {
            // Arrange
            var exepectedThingTypes = Fixture.CreateMany<ThingTypeDto>(24).ToList();
            var thingTypeController = CreateThingTypeController();

            var filter = new DeviceModelFilter
            {
                SearchText = string.Empty,
                PageNumber = 0,
                PageSize = 10,
                OrderBy = new string[]
                {
                    null
                }
            };

            _ = this.mockThingTypeService.Setup(service => service.GetThingTypes(filter))
                .ReturnsAsync((DeviceModelFilter filter) => new PaginatedResult<ThingTypeDto>
                {
                    Data = exepectedThingTypes.Skip(filter.PageSize * filter.PageNumber).Take(filter.PageSize).ToList(),
                    TotalCount = exepectedThingTypes.Count
                });

            var locationUrl = "http://location/aws/thingtypes";

            _ = this.mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(locationUrl);

            // Act
            var response = await thingTypeController.GetThingTypes(filter);

            // Assert
            _ = ((OkObjectResult)response.Result)?.StatusCode.Should().Be(200);
            MockRepository.VerifyAll();

        }

        private ThingTypeController CreateThingTypeController()
        {
            return new ThingTypeController(this.mockThingTypeService.Object)
            {
                Url = this.mockUrlHelper.Object
            };
        }
        /*******======== Test for the avatar=========**********/

        [Test]
        public async Task GetAvatarShouldReturnTheComputedThingTypeAvatarUri()
        {
            // Arrange
            // Arrange
            var thingTypeController = CreateThingTypeController();
            var thingType = Fixture.Create<ThingTypeDto>();
            var expectedAvatar = Fixture.Create<string>();

            _ = this.mockThingTypeService.Setup(service => service.GetThingTypeAvatar(thingType.ThingTypeID))
                .ReturnsAsync(expectedAvatar);

            // Act
            var response = await thingTypeController.GetAvatar(thingType.ThingTypeID);

            // Assert
            _ = ((OkObjectResult)response.Result)?.Value.Should().BeEquivalentTo(expectedAvatar);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task ChangeAvatarShouldChangeThingTypeImageStream()
        {
            // Arrange
            var thingTypeController = CreateThingTypeController();
            var thingType = Fixture.Create<ThingTypeDto>();
            var expectedAvatar = Fixture.Create<string>();

            _ = this.mockThingTypeService.Setup(service => service.UpdateThingTypeAvatar(thingType.ThingTypeID, It.IsAny<IFormFile>()))
                .ReturnsAsync(expectedAvatar);

            // Act
            var response = await thingTypeController.ChangeAvatar(thingType.ThingTypeID, MockRepository.Create<IFormFile>().Object);

            // Assert
            _ = ((OkObjectResult)response.Result)?.Value.Should().BeEquivalentTo(expectedAvatar);
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteAvatarShouldRemoveModelImage()
        {
            // Arrange
            var thingTypeController = CreateThingTypeController();
            var thingType = Fixture.Create<ThingTypeDto>();

            _ = this.mockThingTypeService.Setup(service => service.DeleteThingTypeAvatar(thingType.ThingTypeID))
                .Returns(Task.CompletedTask);

            // Act
            _ = await thingTypeController.DeleteAvatar(thingType.ThingTypeID);

            // Assert
            MockRepository.VerifyAll();
        }


        [Test]
        public async Task CreateAThingTypeShouldReturnOK()
        {
            // Arrange
            var thingTypeController = CreateThingTypeController();

            var thingType = new ThingTypeDto()
            {
                ThingTypeID = Guid.NewGuid().ToString()
            };

            _ = this.mockThingTypeService
                .Setup(x => x.CreateThingType(It.Is<ThingTypeDto>(c => c.ThingTypeID.Equals(thingType.ThingTypeID, StringComparison.Ordinal))))
                .ReturnsAsync(thingType.ThingTypeID);

            // Act
            var response = await thingTypeController.CreateThingTypeAsync(thingType);

            // Assert

            _ = ((OkObjectResult)response.Result)?.Value.Should().BeEquivalentTo(thingType.ThingTypeID);

            this.mockRepository.VerifyAll();
        }
    }
}
