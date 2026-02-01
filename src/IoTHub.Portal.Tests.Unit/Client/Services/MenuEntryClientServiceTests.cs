// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class MenuEntryClientServiceTests : BlazorUnitTest
    {
        private IMenuEntryClientService menuEntryClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IMenuEntryClientService, MenuEntryClientService>();

            this.menuEntryClientService = Services.GetRequiredService<IMenuEntryClientService>();
        }

        [Test]
        public async Task GetMenuEntries_ShouldReturnMenuEntries()
        {
            // Arrange
            var expectedMenuEntries = Fixture.Build<MenuEntryDto>().CreateMany(3).ToList();

            _ = MockHttpClient.When(HttpMethod.Get, "/api/menu-entries")
                .RespondJson(expectedMenuEntries);

            // Act
            var result = await this.menuEntryClientService.GetMenuEntries();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedMenuEntries);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetMenuEntryById_ShouldReturnMenuEntry()
        {
            // Arrange
            var expectedMenuEntry = Fixture.Create<MenuEntryDto>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/menu-entries/{expectedMenuEntry.Id}")
                .RespondJson(expectedMenuEntry);

            // Act
            var result = await this.menuEntryClientService.GetMenuEntryById(expectedMenuEntry.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedMenuEntry);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task CreateMenuEntry_ShouldCreateMenuEntry()
        {
            // Arrange
            var menuEntryDto = Fixture.Create<MenuEntryDto>();
            var createdMenuEntry = Fixture.Build<MenuEntryDto>()
                .With(x => x.Name, menuEntryDto.Name)
                .With(x => x.Url, menuEntryDto.Url)
                .Create();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/menu-entries")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<StringContent>();
                    return true;
                })
                .Respond(HttpStatusCode.Created, new StringContent(
                    JsonSerializer.Serialize(createdMenuEntry),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            // Act
            var result = await this.menuEntryClientService.CreateMenuEntry(menuEntryDto);

            // Assert
            _ = result.Should().NotBeNullOrEmpty();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateMenuEntry_ShouldUpdateMenuEntry()
        {
            // Arrange
            var menuEntryDto = Fixture.Create<MenuEntryDto>();

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/menu-entries/{menuEntryDto.Id}")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<StringContent>();
                    return true;
                })
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.menuEntryClientService.UpdateMenuEntry(menuEntryDto);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteMenuEntry_ShouldDeleteMenuEntry()
        {
            // Arrange
            var menuEntryId = Guid.NewGuid().ToString();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/menu-entries/{menuEntryId}")
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.menuEntryClientService.DeleteMenuEntry(menuEntryId);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateMenuEntryOrder_ShouldUpdateOrder()
        {
            // Arrange
            var menuEntryId = Guid.NewGuid().ToString();
            var newOrder = 5;

            _ = MockHttpClient.When(HttpMethod.Patch, $"/api/menu-entries/{menuEntryId}/order")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<StringContent>();
                    return true;
                })
                .Respond(HttpStatusCode.NoContent);

            // Act
            await this.menuEntryClientService.UpdateMenuEntryOrder(menuEntryId, newOrder);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
