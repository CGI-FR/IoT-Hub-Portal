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
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>()))
                .Returns((Snackbar)null);

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
        public void ClickOnShowAddEdgeModuleDialog()
        {
            // Arrange

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService
                .Setup(c => c.Show<ModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference);

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


    }
}
