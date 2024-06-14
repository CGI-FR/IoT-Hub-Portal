// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Server.Controllers.v10
{
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Server.Controllers.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using UnitTests.Bases;

    [TestFixture]
    public class IdeasControllerTests : BackendUnitTest
    {
        private Mock<IIdeaService> mockIdeaService;

        private IdeasController ideasController;

        public override void Setup()
        {
            base.Setup();

            this.mockIdeaService = MockRepository.Create<IIdeaService>();

            _ = ServiceCollection.AddSingleton(this.mockIdeaService.Object);

            Services = ServiceCollection.BuildServiceProvider();

            this.ideasController = new IdeasController(Services.GetRequiredService<IIdeaService>());
        }

        [Test]
        public async Task SubmitIdeaShouldSubmitIdea()
        {
            // Arrange
            var ideaRequest = Fixture.Create<IdeaRequest>();
            var expectedIdeaResponse = Fixture.Create<IdeaResponse>();

            ideasController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            ideasController.ControllerContext.HttpContext.Request.Headers["idea"] = "test";

            var UA = ideasController.ControllerContext.HttpContext.Request.Headers.UserAgent.ToString();

            _ = this.mockIdeaService.Setup(service => service.SubmitIdea(ideaRequest, UA))
                .ReturnsAsync(expectedIdeaResponse);

            // Act
            var result = await this.ideasController.SubmitIdea(ideaRequest);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedIdeaResponse);
            MockRepository.VerifyAll();
        }
    }
}
