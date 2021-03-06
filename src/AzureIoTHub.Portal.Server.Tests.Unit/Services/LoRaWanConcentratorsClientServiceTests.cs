// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Services;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10.LoRaWAN;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class LoRaWanConcentratorsClientServiceTests : BlazorUnitTest
    {
        private ILoRaWanConcentratorsClientService loRaWanConcentratorsClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<ILoRaWanConcentratorsClientService, LoRaWanConcentratorsClientService>();

            this.loRaWanConcentratorsClientService = Services.GetRequiredService<ILoRaWanConcentratorsClientService>();
        }

        [Test]
        public async Task GetConcentratorsShouldReturnConcentrators()
        {
            // Arrange
            var expectedConcentrators = new PaginationResult<Concentrator>
            {
                Items = new List<Concentrator>()
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

            var expectedConcentrator = new Concentrator();

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
            var concentrator = new Concentrator();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/lorawan/concentrators")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<Concentrator>>();
                    var body = m.Content as ObjectContent<Concentrator>;
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
            var concentrator = new Concentrator();

            _ = MockHttpClient.When(HttpMethod.Put, "/api/lorawan/concentrators")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<Concentrator>>();
                    var body = m.Content as ObjectContent<Concentrator>;
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
    }
}
