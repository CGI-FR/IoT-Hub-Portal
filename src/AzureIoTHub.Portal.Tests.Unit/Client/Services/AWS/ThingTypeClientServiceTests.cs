// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Services.AWS
{
    using AutoFixture;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Services.AWS;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using AzureIoTHub.Portal.Models.v10.AWS;
    using FluentAssertions;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Helpers;

    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using System.Linq;

    [TestFixture]
    public class ThingTypeClientServiceTests : BlazorUnitTest
    {
        private IThingTypeClientService thingTypeClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IThingTypeClientService, ThingTypeClientService>();

            this.thingTypeClientService = Services.GetRequiredService<IThingTypeClientService>();
        }

        [Test]
        public async Task GetThingTypesShouldReturnThingTypes()
        {
            // Arrange
            var expectedThingTypes = new PaginationResult<ThingTypeDto>()
            {
                Items = Fixture.Build<ThingTypeDto>().CreateMany(3).ToList()
            };

            _ = MockHttpClient.When(HttpMethod.Get, "/api/aws/thingtypes?SearchText=&PageNumber=1&PageSize=10&OrderBy=")
                .RespondJson(expectedThingTypes);

            var filter = new DeviceModelFilter
            {
                SearchText = string.Empty,
                PageNumber = 1,
                PageSize = 10,
                OrderBy = new string[]
                {
                    null
                }
            };

            // Act
            var result = await this.thingTypeClientService.GetThingTypes(filter);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedThingTypes);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateThingTypeShouldCreateThingType()
        {
            // Arrange
            var thingType = Fixture.Create<ThingTypeDto>();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/aws/thingtypes")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<ThingTypeDto>>();
                    var body = m.Content as ObjectContent<ThingTypeDto>;
                    _ = body.Value.Should().BeEquivalentTo(thingType);

                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            var response = await thingTypeClientService.CreateThingType(thingType);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();

            _ = response.Should().NotBeNull();
        }


        [Test]
        public async Task GetAvatarUrlShouldReturnAvatarUrl()
        {
            // Arrange
            var thingType = Fixture.Create<ThingTypeDto>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/aws/thingtypes/{thingType.ThingTypeID}/avatar")
                .RespondJson(thingType.ImageUrl.ToString());

            // Act
            var result = await this.thingTypeClientService.GetAvatarUrl(thingType.ThingTypeID);

            // Assert
            _ = result.Should().Contain(thingType.ImageUrl.ToString());
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ChangeAvatarPropertiesShouldChangeAvatar()
        {
            // Arrange
            var thingType = Fixture.Create<ThingTypeDto>();
            using var content = new MultipartFormDataContent();

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/aws/thingtypes/{thingType.ThingTypeID}/avatar")
                .With(m =>
                {
                    _ = m.Content.Should().BeEquivalentTo(content);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.thingTypeClientService.ChangeAvatar(thingType.ThingTypeID, content);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
