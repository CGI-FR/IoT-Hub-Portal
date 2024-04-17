// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using System.Net;
    using UnitTests.Bases;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Helpers;
    using System.Collections.Generic;

    [TestFixture]
    public class LayerClientServiceTests : BlazorUnitTest
    {
        private ILayerClientService layerClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<ILayerClientService, LayerClientService>();

            this.layerClientService = Services.GetRequiredService<ILayerClientService>();
        }

        [Test]
        public async Task CreateLayerShouldCreateLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/building")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<LayerDto>>();
                    var body = m.Content as ObjectContent<LayerDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedLayerDto);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            var result = await this.layerClientService.CreateLayer(expectedLayerDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(result, expectedLayerDto.Id);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateLayerShouldUpdateLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();

            _ = MockHttpClient.When(HttpMethod.Put, "/api/building")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<LayerDto>>();
                    var body = m.Content as ObjectContent<LayerDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedLayerDto);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.layerClientService.UpdateLayer(expectedLayerDto);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteLayerShouldDeleteLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/building/{expectedLayerDto.Id}")
                .Respond(HttpStatusCode.NoContent);

            await this.layerClientService.DeleteLayer(expectedLayerDto.Id);

            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetLayersShouldReturnLayers()
        {
            var expectedDevices = new List<LayerDto>{};

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/building")
                .RespondJson(expectedDevices);

            var result = await this.layerClientService.GetLayers();

            _ = result.Should().BeEquivalentTo(expectedDevices);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetLayerShouldReturnLayer()
        {
            var expectedLayerDto = Fixture.Create<LayerDto>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/building/{expectedLayerDto.Id}")
                .RespondJson(expectedLayerDto);

            // Act
            var result = await this.layerClientService.GetLayer(expectedLayerDto.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedLayerDto);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
