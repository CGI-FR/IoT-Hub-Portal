// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Planning
{
    using IoTHub.Portal.Client.Dialogs.Planning;

    [TestFixture]
    internal class PlanningListPageTest : BlazorUnitTest
    {
        private Mock<IPlanningClientService> mockPlanningClientService;
        private FakeNavigationManager mockNavigationManager;
        private Mock<IDialogService> mockDialogService;
        public override void Setup()
        {
            base.Setup();

            this.mockPlanningClientService = MockRepository.Create<IPlanningClientService>();
            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockPlanningClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
            _ = Services.AddSingleton(this.mockDialogService.Object);

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void OnInitializedAsync_ListPlanningPage()
        {
            var expectedPlanningsNumber = 10;
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
            var expectedPlanningsNumber = 10;
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
            var expectedPlanningsNumber = 10;
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

        [Test]
        public void DeletePlanning_Clicked_DeletePlanningDialogIsShown()
        {
            // Arrange
            var planningId = Guid.NewGuid().ToString();

            _ = this.mockPlanningClientService.Setup(service =>
                    service.GetPlannings()).ReturnsAsync(new List<PlanningDto>
                    {
                        new PlanningDto() { Id = planningId, Name = Guid.NewGuid().ToString() }
                    });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeletePlanningDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<PlanningListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForElement($"#delete-planning-{planningId}").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
