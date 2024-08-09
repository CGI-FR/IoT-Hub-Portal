// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Planning
{
    using Bunit;
    using IoTHub.Portal.Client.Pages.Planning;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using AutoFixture;
    using System.Linq;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Exceptions;
    using Bunit.TestDoubles;
    using FluentAssertions;

    [TestFixture]
    internal class PlanningListPageTest : BlazorUnitTest
    {
        private Mock<IPlanningClientService> mockPlanningClientService;
        private FakeNavigationManager mockNavigationManager;
        public override void Setup()
        {
            base.Setup();

            this.mockPlanningClientService = MockRepository.Create<IPlanningClientService>();

            _ = Services.AddSingleton(this.mockPlanningClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void OnInitializedAsync_ListPlanningPage()
        {
            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<PlanningListPage>();

            // Assert
            Assert.AreEqual(cut.Instance.IsLoading, false);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_ErrorListPlanningPage()
        {
            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<PlanningListPage>();

            // Assert
            Assert.AreEqual(cut.Instance.IsLoading, false);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_ListNewPlanningPage()
        {
            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<PlanningListPage>();

            var planningListAddLayer = cut.WaitForElement("#planningListAddLayer");
            planningListAddLayer.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/planning/new"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsync_ListDetailPlanningPage()
        {
            int expectedPlanningsNumber = 10;
            var expectedPlannings = Fixture.CreateMany<PlanningDto>(expectedPlanningsNumber).ToList();

            _ = this.mockPlanningClientService.Setup(service => service.GetPlannings())
                .ReturnsAsync(expectedPlannings);

            // Act
            var cut = RenderComponent<PlanningListPage>();

            var planningListDetailDetail = cut.WaitForElement("#planningListDetailDetail");
            planningListDetailDetail.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith($"/planning/{expectedPlannings[0].Id}"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
