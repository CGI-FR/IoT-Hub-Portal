// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
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
    }
}
