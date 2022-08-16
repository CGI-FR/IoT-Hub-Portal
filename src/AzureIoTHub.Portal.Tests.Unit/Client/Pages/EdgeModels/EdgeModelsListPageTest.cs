// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.EdgeModels
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

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
            _ = this.mockEdgeModelServiceClient.Setup(x => x.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = Guid.NewGuid().ToString() },
                });

            // Act
            var cut = RenderComponent<EdgeDeviceModelListPage>();
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
        public void WhenAddNewDeviceModelClickShouldNavigateToNewDeviceModelPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = Guid.NewGuid().ToString() },
                });

            // Act
            var cut = RenderComponent<EdgeDeviceModelListPage>();

            cut.WaitForElement("#addEdgeModelButton").Click();
            cut.WaitForState(() => Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("edge/models/new", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenRefreshClickShouldReloadFromApi()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = Guid.NewGuid().ToString() },
                });

            // Act
            var cut = RenderComponent<EdgeDeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Find("#tableRefreshButton"));

            for (var i = 0; i < 3; i++)
            {
                cut.WaitForElement("#tableRefreshButton").Click();
            }

            // Assert
            cut.WaitForAssertion(() => this.mockEdgeModelServiceClient.Verify(service => service.GetIoTEdgeModelList(), Times.Exactly(4)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = Guid.NewGuid().ToString() },
                });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService.Setup(c => c.Show<DeleteEdgeDeviceModelDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeDeviceModelListPage>();

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

            _ = this.mockEdgeModelServiceClient.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem() { ModelId = Guid.NewGuid().ToString() },
                });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService.Setup(c => c.Show<DeleteEdgeDeviceModelDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeDeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockEdgeModelServiceClient.Verify(service => service.GetIoTEdgeModelList(), Times.Exactly(2)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
