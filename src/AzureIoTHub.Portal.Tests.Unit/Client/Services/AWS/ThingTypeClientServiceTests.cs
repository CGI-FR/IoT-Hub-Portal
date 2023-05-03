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
            _ = await this.thingTypeClientService.CreateThingType(thingType);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
