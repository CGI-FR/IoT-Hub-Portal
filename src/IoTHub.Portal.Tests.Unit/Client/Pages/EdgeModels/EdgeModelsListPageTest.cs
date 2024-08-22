// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.EdgeModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.EdgeModels;
    using IoTHub.Portal.Client.Dialogs.EdgeModels;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;
    using Portal.Shared.Models.v1._0.Filters;

    [TestFixture]
    public class EdgeModelsListPageTest : BlazorUnitTest
    {
        private Mock<IEdgeModelClientService> mockEdgeModelServiceClient;
        private Mock<IDialogService> mockDialogService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeModelServiceClient = MockRepository.Create<IEdgeModelClientService>();
            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockEdgeModelServiceClient.Object);
            _ = Services.AddSingleton(this.mockDialogService.Object);
        }

        [Test]
        public void EdgeModelListPageRendersCorrectly()
        {
            // Arrange
            _ = this.mockEdgeModelServiceClient.Setup(x => x.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = Guid.NewGuid().ToString() },
                });

            // Act
            var cut = RenderComponent<EdgeModelListPage>();
            var grid = cut.WaitForElement("div.mud-grid");

            // Assert
            Assert.IsNotNull(cut.Markup);
            Assert.IsNotNull(grid.InnerHtml);

            cut.WaitForAssertion(() => Assert.AreEqual("Edge Models", cut.Find(".mud-typography-h6").TextContent));
            cut.WaitForAssertion(() => Assert.AreEqual(2, cut.FindAll("tr").Count));
            cut.WaitForAssertion(() => Assert.IsNotNull(cut.Find(".mud-table-container")));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SearchEdgeModels_ClickOnSearch_EdgeModelsAreSearched()
        {
            // Arrange
            var searchKeyword = Fixture.Create<string>();

            _ = this.mockEdgeModelServiceClient.Setup(x => x.GetIoTEdgeModelList(It.Is<EdgeModelFilter>(x => string.IsNullOrEmpty(x.Keyword))))
                .ReturnsAsync(Fixture.CreateMany<IoTEdgeModelListItem>(3).ToList());

            _ = this.mockEdgeModelServiceClient.Setup(x => x.GetIoTEdgeModelList(It.Is<EdgeModelFilter>(x => searchKeyword.Equals(x.Keyword, StringComparison.Ordinal))))
                .ReturnsAsync(Fixture.CreateMany<IoTEdgeModelListItem>(2).ToList());

            var cut = RenderComponent<EdgeModelListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(3));
            cut.WaitForElement("#edge-model-search-keyword").Change(searchKeyword);

            // Act
            cut.WaitForElement("#edge-model-search-button").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickToItemShouldRedirectToDetailsPage()
        {
            // Arrange
            var modelId = Fixture.Create<string>();

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = modelId},
                });

            var cut = RenderComponent<EdgeModelListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>()?.Uri.Should().EndWith($"/edge/models/{modelId}"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenAddNewDeviceModelClickShouldNavigateToNewDeviceModelPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = deviceId },
                });

            // Act
            var cut = RenderComponent<EdgeModelListPage>();

            cut.WaitForElement("#addEdgeModelButton").Click();
            cut.WaitForState(() => Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("edge/models/new", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LoadDeviceModelsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            // Arrange
            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EdgeModelListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenRefreshClickShouldReloadFromApi()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = deviceId },
                });

            // Act
            var cut = RenderComponent<EdgeModelListPage>();
            cut.WaitForAssertion(() => cut.Find("#tableRefreshButton"));

            for (var i = 0; i < 3; i++)
            {
                cut.WaitForElement("#tableRefreshButton").Click();
            }

            // Assert
            cut.WaitForAssertion(() => this.mockEdgeModelServiceClient.Verify(service => service.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()), Times.Exactly(4)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = deviceId },
                });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService.Setup(c => c.Show<DeleteEdgeModelDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReloadDeviceModelIfConfirmed()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = deviceId },
                });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService.Setup(c => c.Show<DeleteEdgeModelDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockEdgeModelServiceClient.Verify(service => service.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()), Times.Exactly(2)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSortLabel()
        {
            // Arrange
            _ = this.mockEdgeModelServiceClient.Setup(x => x.GetIoTEdgeModelList(It.IsAny<EdgeModelFilter>()))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        ModelId = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString()
                    },
                    new IoTEdgeModelListItem()
                    {
                        ModelId = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString()
                    },
                });

            // Act
            var cut = RenderComponent<EdgeModelListPage>();

            cut.WaitForAssertion(() => cut.WaitForElement("#NameLabel").Should().NotBeNull());
            var sortNameButtons = cut.WaitForElement("#NameLabel");
            sortNameButtons.Click();

            cut.WaitForAssertion(() => cut.WaitForElement("#DescriptionLabel").Should().NotBeNull());
            var sortDescriptionButtons = cut.WaitForElement("#DescriptionLabel");
            sortDescriptionButtons.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(3, cut.FindAll("tr").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
