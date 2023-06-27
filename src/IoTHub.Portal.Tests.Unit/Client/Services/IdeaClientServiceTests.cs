// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using UnitTests.Bases;
    using UnitTests.Helpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Portal.Client.Services;
    using RichardSzalay.MockHttp;
    using Shared.Models.v1._0;

    [TestFixture]
    public class IdeaClientServiceTests : BlazorUnitTest
    {
        private IIdeaClientService ideaClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IIdeaClientService, IdeaClientService>();

            this.ideaClientService = Services.GetRequiredService<IIdeaClientService>();
        }

        [Test]
        public async Task SubmitIdeaShouldSubmitIdea()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequestDto>();
            var expectedIdeaResponse = Fixture.Create<IdeaResponseDto>();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/ideas")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IdeaRequestDto>>();
                    var body = (ObjectContent<IdeaRequestDto>) m.Content;
                    _ = body?.Value.Should().BeEquivalentTo(ideaRequest);
                    return true;
                })
                .RespondJson(expectedIdeaResponse);

            // Act
            var result = await this.ideaClientService.SubmitIdea(ideaRequest);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedIdeaResponse);
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockHttpClient.VerifyNoOutstandingRequest();
        }
    }
}
