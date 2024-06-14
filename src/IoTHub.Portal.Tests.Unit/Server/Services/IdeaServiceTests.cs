// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Server.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using UAParser;
    using UnitTests.Bases;
    using UnitTests.Helpers;

    [TestFixture]
    public class IdeaServiceTests : BackendUnitTest
    {
        private Mock<ConfigHandler> mockConfigHandler;

        private IIdeaService ideaService;

        public override void Setup()
        {
            base.Setup();


            this.mockConfigHandler = MockRepository.Create<ConfigHandler>();

            _ = ServiceCollection.AddSingleton(this.mockConfigHandler.Object);
            _ = ServiceCollection.AddSingleton<IIdeaService, IdeaService>();

            Services = ServiceCollection.BuildServiceProvider();

            this.ideaService = Services.GetRequiredService<IIdeaService>();
        }

        [Test]
        public async Task SubmitIdeaShouldSubmitIdea()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();
            var expectedIdeaResponse = Fixture.Create<IdeaResponse>();
            var uaString = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_1_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B206 Safari/7534.48.3";
            var uaParser = Fixture.Create<Parser>();

            var c = uaParser.Parse(uaString);

            var submitIdea = new SubmitIdeaRequest();

            var description = new StringBuilder();
            _ = description.Append("Description: ");
            _ = description.Append(ideaRequest.Body);
            _ = description.AppendLine();
            _ = description.Append("Application Version: ");
            _ = description.Append(Assembly.GetExecutingAssembly().GetName().Version);
            _ = description.AppendLine();
            _ = description.Append("Browser Version: ");
            _ = description.Append(string.Concat(c.UA.Family, c.UA.Major, c.UA.Minor));

            submitIdea.Title = ideaRequest.Title;
            submitIdea.Description = description.ToString();

            _ = this.mockConfigHandler.Setup(handler => handler.IdeasEnabled)
                .Returns(true);

            _ = MockHttpClient.When(HttpMethod.Post, "/ideas")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<StringContent>();
                    var stringBody = (StringContent) m.Content;
                    _ = stringBody.Should().NotBeNull();
                    return true;
                })
                .RespondJson(expectedIdeaResponse);

            // Act
            var result = await this.ideaService.SubmitIdea(ideaRequest, uaString);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedIdeaResponse);
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SubmitIdeaShouldThrowInternalServerErrorExceptionWhenIssueOccursOnSubmittingIdea()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();

            var uaString = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_1_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B206 Safari/7534.48.3";

            _ = this.mockConfigHandler.Setup(handler => handler.IdeasEnabled)
                .Returns(true);

            _ = MockHttpClient.When(HttpMethod.Post, "/ideas")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<StringContent>();
                    var stringBody = (StringContent) m.Content;
                    _ = stringBody.Should().NotBeNull();
                    return true;
                })
                .Respond(HttpStatusCode.InternalServerError);

            // Act
            var act = () => this.ideaService.SubmitIdea(ideaRequest, uaString);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SubmitIdeaShouldThrowInternalServerErrorExceptionWhenIdeasFeatureIsNotEnabled()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();

            _ = this.mockConfigHandler.Setup(handler => handler.IdeasEnabled)
                .Returns(false);

            // Act
            var act = () => this.ideaService.SubmitIdea(ideaRequest);

            // Assert
            _ = await act.Should().ThrowAsync<InternalServerErrorException>();
            MockRepository.VerifyAll();
        }

        [Test]
        public async Task SubmitIdeaWithoutConsentShouldSubmitIdea()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();
            var expectedIdeaResponse = Fixture.Create<IdeaResponse>();
            var uaString = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_1_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B206 Safari/7534.48.3";

            ideaRequest.ConsentToCollectTechnicalDetails = false;

            _ = this.mockConfigHandler.Setup(handler => handler.IdeasEnabled)
                .Returns(true);

            _ = MockHttpClient.When(HttpMethod.Post, "/ideas")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<StringContent>();
                    var stringBody = (StringContent) m.Content;
                    _ = stringBody.Should().NotBeNull();
                    return true;
                })
                .RespondJson(expectedIdeaResponse);

            // Act
            var result = await this.ideaService.SubmitIdea(ideaRequest, uaString);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedIdeaResponse);
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockRepository.VerifyAll();
        }
    }
}
