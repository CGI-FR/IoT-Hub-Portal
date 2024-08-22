// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using UnitTests.Helpers;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using Shared;
    using Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class LoRaWanConcentratorsClientServiceTests : BlazorUnitTest
    {
        private ILoRaWanConcentratorClientService loRaWanConcentratorsClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<ILoRaWanConcentratorClientService, LoRaWanConcentratorClientService>();

            this.loRaWanConcentratorsClientService = Services.GetRequiredService<ILoRaWanConcentratorClientService>();
        }

        [Test]
        public async Task GetConcentratorsShouldReturnConcentrators()
        {
            // Arrange
            var expectedConcentrators = new PaginationResult<ConcentratorDto>
            {
                Items = new List<ConcentratorDto>()
                {
                    new ()
                }
            };

            var expectedUri = "/api/lorawan/concentrators?pageSize=10";

            _ = MockHttpClient.When(HttpMethod.Get, expectedUri)
                .RespondJson(expectedConcentrators);

            // Act
            var result = await this.loRaWanConcentratorsClientService.GetConcentrators(expectedUri);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConcentrators);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetConcentratorShouldReturnConcentrator()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            var expectedConcentrator = new ConcentratorDto();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/lorawan/concentrators/{deviceId}")
                .RespondJson(expectedConcentrator);

            // Act
            var result = await this.loRaWanConcentratorsClientService.GetConcentrator(deviceId);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedConcentrator);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateConcentratorShouldCreateConcentrator()
        {
            // Arrange
            var concentrator = new ConcentratorDto();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/lorawan/concentrators")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<ConcentratorDto>>();
                    var body = m.Content as ObjectContent<ConcentratorDto>;
                    _ = body.Value.Should().BeEquivalentTo(concentrator);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanConcentratorsClientService.CreateConcentrator(concentrator);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateConcentratorShouldUpdateConcentrator()
        {
            // Arrange
            var concentrator = new ConcentratorDto();

            _ = MockHttpClient.When(HttpMethod.Put, "/api/lorawan/concentrators")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<ConcentratorDto>>();
                    var body = m.Content as ObjectContent<ConcentratorDto>;
                    _ = body.Value.Should().BeEquivalentTo(concentrator);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.loRaWanConcentratorsClientService.UpdateConcentrator(concentrator);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteConcentratorShouldDeleteConcentrator()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/lorawan/concentrators/{deviceId}")
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.loRaWanConcentratorsClientService.DeleteConcentrator(deviceId);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetLoRaWANFrequencyPlansShouldQueryTheController()
        {
            // Arrange
            var expectedFrequencyPlans = new []
            {
                    new FrequencyPlan(),
                    new FrequencyPlan(),
                    new FrequencyPlan()
            };

            _ = MockHttpClient.When(HttpMethod.Get, "/api/lorawan/freqencyplans")
                .RespondJson(expectedFrequencyPlans);

            // Act
            var result = await this.loRaWanConcentratorsClientService.GetFrequencyPlans();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedFrequencyPlans);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
