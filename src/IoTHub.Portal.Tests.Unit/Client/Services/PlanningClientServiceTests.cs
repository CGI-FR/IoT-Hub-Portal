// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    [TestFixture]
    public class PlanningClientServiceTests : BlazorUnitTest
    {
        private IPlanningClientService planningClientService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IPlanningClientService, PlanningClientService>();

            this.planningClientService = Services.GetRequiredService<IPlanningClientService>();
        }

        [Test]
        public async Task CreatePlanningShouldCreatePlanning()
        {
            var expectedPlanningDto = Fixture.Create<PlanningDto>();

            _ = MockHttpClient.When(HttpMethod.Post, "/api/planning")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<PlanningDto>>();
                    var body = m.Content as ObjectContent<PlanningDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedPlanningDto);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            var result = await this.planningClientService.CreatePlanning(expectedPlanningDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(result, expectedPlanningDto.Id);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdatePlanningShouldUpdatePlanning()
        {
            var expectedPlanningDto = Fixture.Create<PlanningDto>();

            _ = MockHttpClient.When(HttpMethod.Put, "/api/planning")
                .With(m =>
                {
                    _ = m.Content.Should().BeAssignableTo<ObjectContent<PlanningDto>>();
                    var body = m.Content as ObjectContent<PlanningDto>;
                    _ = body.Value.Should().BeEquivalentTo(expectedPlanningDto);
                    return true;
                })
                .Respond(HttpStatusCode.Created);

            // Act
            await this.planningClientService.UpdatePlanning(expectedPlanningDto);

            // Assert
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task DeletePlanningShouldDeletePlanning()
        {
            var expectedPlanningDto = Fixture.Create<PlanningDto>();

            _ = MockHttpClient.When(HttpMethod.Delete, $"/api/planning/{expectedPlanningDto.Id}")
                .Respond(HttpStatusCode.NoContent);

            await this.planningClientService.DeletePlanning(expectedPlanningDto.Id);

            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetPlanningsShouldReturnPlannings()
        {
            var expectedDevices = new List<PlanningDto>{};

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/planning")
                .RespondJson(expectedDevices);

            var result = await this.planningClientService.GetPlannings();

            _ = result.Should().BeEquivalentTo(expectedDevices);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task GetPlanningShouldReturnPlanning()
        {
            var expectedPlanningDto = Fixture.Create<PlanningDto>();

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/planning/{expectedPlanningDto.Id}")
                .RespondJson(expectedPlanningDto);

            // Act
            var result = await this.planningClientService.GetPlanning(expectedPlanningDto.Id);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedPlanningDto);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
