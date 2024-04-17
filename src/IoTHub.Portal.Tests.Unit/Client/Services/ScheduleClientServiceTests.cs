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
    public class ScheduleClientServiceTests : BlazorUnitTest
    {
        private IScheduleClientService scheduleClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IScheduleClientService, ScheduleClientService>();

            this.scheduleClientService = Services.GetRequiredService<IScheduleClientService>();
        }

        [Test]
        public async Task CreateScheduleShouldCreateSchedule()
        {
            var expectedScheduleDto = Fixture.Create<ScheduleDto>();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/schedule")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<ScheduleDto>>();
                    var body = m.Content as ObjectContent<ScheduleDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedScheduleDto);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            var result = await this.scheduleClientService.CreateSchedule(expectedScheduleDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(result, expectedScheduleDto.Id);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateScheduleShouldUpdateSchedule()
        {
            var expectedScheduleDto = Fixture.Create<ScheduleDto>();

            _ = MockHttpClient.When(HttpMethod.Put, "/api/schedule")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<ScheduleDto>>();
                    var body = m.Content as ObjectContent<ScheduleDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedScheduleDto);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.scheduleClientService.UpdateSchedule(expectedScheduleDto);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeleteScheduleShouldDeleteSchedule()
        {
            var expectedScheduleDto = Fixture.Create<ScheduleDto>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/schedule/{expectedScheduleDto.Id}")
                .Respond(HttpStatusCode.NoContent);

            await this.scheduleClientService.DeleteSchedule(expectedScheduleDto.Id);

            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetSchedulesShouldReturnSchedules()
        {
            var expectedDevices = new List<ScheduleDto>{};

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/schedule")
                .RespondJson(expectedDevices);

            var result = await this.scheduleClientService.GetSchedules();

            _ = result.Should().BeEquivalentTo(expectedDevices);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetScheduleShouldReturnSchedule()
        {
            var expectedScheduleDto = Fixture.Create<ScheduleDto>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/schedule/{expectedScheduleDto.Id}")
                .RespondJson(expectedScheduleDto);

            // Act
            var result = await this.scheduleClientService.GetSchedule(expectedScheduleDto.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedScheduleDto);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
