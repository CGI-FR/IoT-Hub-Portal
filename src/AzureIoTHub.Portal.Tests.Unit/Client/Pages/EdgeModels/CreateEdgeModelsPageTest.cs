// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.EdgeModels
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels.EdgeModule;
    using AzureIoTHub.Portal.Client.Services;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class CreateEdgeModelsPageTest : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IEdgeModelClientService> mockEdgeModelClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockEdgeModelClientService = MockRepository.Create<IEdgeModelClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockEdgeModelClientService.Object);
        }

        [Test]
        public void ClickOnSaveShouldPostEdgeModelData()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel()
            {
                Name = Guid.NewGuid().ToString(),
            };

            _ = this.mockEdgeModelClientService
                .Setup(x => x.CreateIoTEdgeModel(It.Is<IoTEdgeModel>(x => edgeModel.Name.Equals(x.Name, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeModel.Name)}").Change(edgeModel.Name);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/edge/models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenEdgeModelRequiredFieldEmptyClickOnSaveShouldProssessValidationError()
        {
            // Arrange

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenModulesRequiredFieldEmptyClickOnSaveShouldProssessValidationError()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel()
            {
                Name = Guid.NewGuid().ToString(),
            };

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeModel.Name)}").Change(edgeModel.Name);

            var addModuleButton = cut.WaitForElement("#addModuleButton");
            addModuleButton.Click();

            cut.WaitForElement($"#{nameof(IoTEdgeModule.ModuleName)}").Change("module_test");

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenRoutesRequiredFieldEmptyClickOnSaveShouldProssessValidationError()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel()
            {
                Name = Guid.NewGuid().ToString(),
            };

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeModel.Name)}").Change(edgeModel.Name);

            var addRouteButton = cut.WaitForElement("#addRouteButton");
            addRouteButton.Click();

            cut.WaitForElement($"#{nameof(IoTEdgeRoute.Name)}").Change("RouteTest");

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldPostProcessProblemDetailsExceptionWhenIssueOccurs()
        {
            // Arrange
            var edgeModel = new IoTEdgeModel()
            {
                Name = Guid.NewGuid().ToString(),
            };

            _ = this.mockEdgeModelClientService
                .Setup(x => x.CreateIoTEdgeModel(It.Is<IoTEdgeModel>(x => edgeModel.Name.Equals(x.Name, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeModel.Name)}").Change(edgeModel.Name);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddModuleShouldAddModuleOnEdgeModelData()
        {
            // Arrange

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var addModuleButton = cut.WaitForElement("#addModuleButton");

            addModuleButton.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteModuleButton").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteModuleShouldRemoveModule()
        {
            // Arrange

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var addModuleButton = cut.WaitForElement("#addModuleButton");

            addModuleButton.Click();
            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteModuleButton").Count));

            var deleteModuleButton = cut.WaitForElement(".deleteModuleButton");

            deleteModuleButton.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(0, cut.FindAll(".deleteModuleButton").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnShowAddEdgeModuleDialogShouldShowDialog()
        {
            // Arrange
            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService
                .Setup(c => c.Show<ModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();

            var addModuleButton = cut.WaitForElement("#addModuleButton");

            addModuleButton.Click();
            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteModuleButton").Count));

            var editButton = cut.WaitForElement("#editButton");

            cut.WaitForElement($"#{nameof(IoTEdgeModule.ModuleName)}").Change("module test");
            cut.WaitForElement($"#{nameof(IoTEdgeModule.ImageURI)}").Change("image test");

            editButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickShowAddEdgeModuleDialogShouldDisplayAddModuleDialogAndReturnIfAborted()
        {
            // Arrange
            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService
                .Setup(c => c.Show<ModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<CreateEdgeModelsPage>();

            // Act
            var addModuleButton = cut.WaitForElement("#addModuleButton");

            addModuleButton.Click();
            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteModuleButton").Count));

            var editButton = cut.WaitForElement("#editButton");

            cut.WaitForElement($"#{nameof(IoTEdgeModule.ModuleName)}").Change("module test");
            cut.WaitForElement($"#{nameof(IoTEdgeModule.ImageURI)}").Change("image test");

            editButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnShowSystemModuleDetailShouldShowDialog()
        {
            // Arrange
            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService
                .Setup(c => c.Show<SystemModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();

            var editEdgeAgentButton = cut.WaitForElement("#editSystModuleButton_edgeAgent");

            cut.WaitForElement($"#{nameof(EdgeModelSystemModule.Name)}").Change("newTest");
            cut.WaitForElement($"#{nameof(EdgeModelSystemModule.ImageUri)}").Change("image/test");

            editEdgeAgentButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnShowSystemModuleDetailShouldShowDialogAndReturnIfAborted()
        {
            // Arrange
            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService
                .Setup(c => c.Show<SystemModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();

            var editEdgeAgentButton = cut.WaitForElement("#editSystModuleButton_edgeAgent");

            cut.WaitForElement($"#{nameof(EdgeModelSystemModule.ImageUri)}").Change("image/test");

            editEdgeAgentButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddRouteShouldAddRouteOnEdgeModelData()
        {
            // Arrange

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var addRouteButton = cut.WaitForElement("#addRouteButton");

            addRouteButton.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteRouteButton").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteRouteShouldRemoveRouteFromEdgeModelData()
        {
            // Arrange

            // Act
            var cut = RenderComponent<CreateEdgeModelsPage>();
            var addRouteButton = cut.WaitForElement("#addRouteButton");

            addRouteButton.Click();
            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteRouteButton").Count));

            var deleteRouteButton = cut.WaitForElement(".deleteRouteButton");

            deleteRouteButton.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(0, cut.FindAll(".deleteRouteButton").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
