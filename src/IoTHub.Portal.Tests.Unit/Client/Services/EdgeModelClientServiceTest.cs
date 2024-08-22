// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using IoTHub.Portal.Tests.Unit.UnitTests.Helpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using Shared.Models.v1._0;

    [TestFixture]
    public class EdgeModelClientServiceTest : BlazorUnitTest
    {
        private IEdgeModelClientService edgeModelClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IEdgeModelClientService, EdgeModelClientService>();

            this.edgeModelClientService = Services.GetRequiredService<IEdgeModelClientService>();
        }

        [Test]
        public async Task GetIoTEdgeModelShouldReturnEdgeModel()
        {
            // Arrange
            var modelId = Fixture.Create<string>();

            var expectedModel = new IoTEdgeModel
            {
                ModelId = modelId
            };

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/edge/models/{modelId}")
                .RespondJson(expectedModel);

            // Act
            var result = await this.edgeModelClientService.GetIoTEdgeModel(modelId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedModel);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetIoTEdgeModelListShouldReturnList()
        {
            var expectedModels = Fixture.Build<IoTEdgeModelListItem>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/edge/models")
                .RespondJson(expectedModels);

            // Act
            var result = await this.edgeModelClientService.GetIoTEdgeModelList();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedModels);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateIoTEdgeModelShouldCreateEdgeModel()
        {
            // Arrange
            var expectedEdgeModel = Fixture.Create<IoTEdgeModel>();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/edge/models")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IoTEdgeModel>>();

                    if (m.Content is ObjectContent<IoTEdgeModel> body)
                    {
                        _ = body.Value.Should().BeEquivalentTo(expectedEdgeModel);
                    }

                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.edgeModelClientService.CreateIoTEdgeModel(expectedEdgeModel);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateIoTEdgeModelShouldUpdateEdgeModel()
        {
            // Arrange
            var expectedEdgeModel = Fixture.Create<IoTEdgeModel>();

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/edge/models")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<IoTEdgeModel>>();

                    if (m.Content is ObjectContent<IoTEdgeModel> body)
                    {
                        _ = body.Value.Should().BeEquivalentTo(expectedEdgeModel);
                    }
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.edgeModelClientService.UpdateIoTEdgeModel(expectedEdgeModel);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteIoTEdgeModelShouldDeleteEdgeModel()
        {
            // Arrange
            var expectedEdgeModel = Fixture.Create<DeviceModelDto>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/edge/models/{expectedEdgeModel.ModelId}")
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.edgeModelClientService.DeleteIoTEdgeModel(expectedEdgeModel.ModelId);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetAvatarUrlShouldReturnAvatarUrl()
        {
            // Arrange
            var edgeModel = Fixture.Create<IoTEdgeModel>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/edge/models/{edgeModel.ModelId}/avatar")
                .RespondJson(edgeModel.ImageUrl.ToString());

            // Act
            var result = await this.edgeModelClientService.GetAvatarUrl(edgeModel.ModelId);

            // Assert
            _ = result.Should().Contain(edgeModel.ImageUrl.ToString());
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ChangeAvatarPropertiesShouldChangeAvatar()
        {
            // Arrange
            var deviceModel = Fixture.Create<IoTEdgeModel>();
            using var content = new MultipartFormDataContent();

            _ = MockHttpClient.When(HttpMethod.Post, $"/api/edge/models/{deviceModel.ModelId}/avatar")
                .With(m =>
                {
                    _ = m.Content.Should().BeEquivalentTo(content);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.edgeModelClientService.ChangeAvatar(deviceModel.ModelId, content);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteAvatarPropertiesShouldChangeAvatar()
        {
            // Arrange
            var deviceModel = Fixture.Create<IoTEdgeModel>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/edge/models/{deviceModel.ModelId}/avatar")
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.edgeModelClientService.DeleteAvatar(deviceModel.ModelId);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetPublicEdgeModules_GetPublicEdgeModules_EdgeModulesReturned()
        {
            // Arrange
            var expectedModels = Fixture.Build<IoTEdgeModule>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/edge/models/public-modules")
                .RespondJson(expectedModels);

            // Act
            var result = await this.edgeModelClientService.GetPublicEdgeModules();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedModels);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
