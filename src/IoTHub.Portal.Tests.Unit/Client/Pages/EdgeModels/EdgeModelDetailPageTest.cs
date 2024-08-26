// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.EdgeModels
{
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Portal.Client.Dialogs.EdgeModels;
    using Portal.Client.Dialogs.EdgeModels.EdgeModule;
    using Portal.Client.Pages.EdgeModels;
    using Portal.Shared.Constants;

    [TestFixture]
    public class EdgeModelDetailPageTest : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IEdgeModelClientService> mockEdgeModelService;

        private readonly string mockEdgeModleId = Guid.NewGuid().ToString();

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeModelService = MockRepository.Create<IEdgeModelClientService>();
            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();

            _ = Services.AddSingleton(this.mockEdgeModelService.Object);
            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });
        }

        [Test]
        public void ClickOnReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            // Act
            cut.WaitForElement("#returnButton").Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/edge/models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveChangesShouldUpdateTheData()
        {
            // Arrange

            var edgeModel = SetupLoadEdgeModel();

            _ = this.mockEdgeModelService
                .Setup(x => x.UpdateIoTEdgeModel(It.Is<IoTEdgeModel>(c => c.ModelId.Equals(edgeModel.ModelId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService
                .Setup(c => c.Add("Edge model successfully updated.", Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            _ = this.mockEdgeModelService.Setup(service =>
                    service.ChangeAvatar(It.IsAny<string>(), It.IsAny<StringContent>()))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            // Act
            cut.WaitForElement("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenModuleRequiredFieldEmptyClickOnSaveShouldProcessValidationError()
        {
            // Arrange

            var edgeModel =  SetupLoadEdgeModel();

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));
            var saveButton = cut.WaitForElement("#saveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeModel.Name)}").Change(edgeModel.Name);

            var addModuleButton = cut.WaitForElement("#addModuleButton");
            addModuleButton.Click();

            cut.WaitForElement($"#{nameof(IoTEdgeRoute.Name)}").Change("ModuleTest");

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenRoutesRequiredFieldEmptyClickOnSaveShouldProcessValidationError()
        {
            // Arrange

            var edgeModel = SetupLoadEdgeModel();

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));
            var saveButton = cut.WaitForElement("#saveButton");

            cut.WaitForElement($"#{nameof(IoTEdgeModel.Name)}").Change(edgeModel.Name);

            var addRouteButton = cut.WaitForElement("#addRouteButton");
            addRouteButton.Click();

            cut.WaitForElement($"#{nameof(IoTEdgeRoute.Name)}").Change("RouteTest");

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickOnSaveChangesShouldProssessProblem()
        {
            // Arrange
            var edgeModel =  SetupLoadEdgeModel();

            _ = this.mockEdgeModelService
                .Setup(x => x.UpdateIoTEdgeModel(It.Is<IoTEdgeModel>(c => c.ModelId.Equals(edgeModel.ModelId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            // Act
            cut.WaitForElement("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddModuleShouldAddModuleOnEdgeModelData()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));
            var addModuleButton = cut.WaitForElement("#addModuleButton");

            addModuleButton.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(2, cut.FindAll(".deleteModuleButton").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteModuleShouldRemoveModule()
        {
            // Arrange
            _ = SetupLoadEdgeModel();

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteModuleButton").Count));

            var deleteModuleButton = cut.WaitForElement(".deleteModuleButton");

            deleteModuleButton.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(0, cut.FindAll(".deleteModuleButton").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteEdgeModelButtonShouldShowDeleteDialogAndRedirectIfOk()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService
                .Setup(c => c.Show<DeleteEdgeModelDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            var deleteModelBtn = cut.WaitForElement("#deleteButton");
            deleteModelBtn.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/edge/models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteEdgeModelButtonShouldShowDeleteDialogAndReturnIfAborted()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService
                .Setup(c => c.Show<DeleteEdgeModelDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            var deleteModelBtn = cut.WaitForElement("#deleteButton");
            deleteModelBtn.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnShowEditEdgeModuleDialogShouldShowDialog()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService
                .Setup(c => c.Show<ModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteModuleButton").Count));

            var editButton = cut.WaitForElement("#editButton");

            editButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickShowEditEdgeModuleDialogShouldDisplayEditModuleDialogAndReturnIfAborted()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService
                .Setup(c => c.Show<ModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteModuleButton").Count));

            var editButton = cut.WaitForElement("#editButton");

            editButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnShowSystemModuleDetailShouldShowDialog()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService
                .Setup(c => c.Show<SystemModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll("#editSystModuleButton_edgeAgent").Count));
            var editEdgeAgentButton = cut.WaitForElement("#editSystModuleButton_edgeAgent");

            cut.WaitForElement($"#{nameof(EdgeModelSystemModule.Name)}-image").Change("image/test");

            editEdgeAgentButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnShowSystemModuleDetailShouldShowDialogAndReturnIfAborted()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService
                .Setup(c => c.Show<SystemModuleDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll("#editSystModuleButton_edgeAgent").Count));
            var editEdgeAgentButton = cut.WaitForElement("#editSystModuleButton_edgeAgent");

            cut.WaitForElement($"#{nameof(EdgeModelSystemModule.Name)}-image").Change("image/test");

            editEdgeAgentButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeleteAvatarShouldRemoveTheImage()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            cut.WaitForAssertion(() => Assert.IsFalse(string.IsNullOrEmpty(cut.Find($"#model-image").Attributes["src"]?.Value)));

            var avatar = cut.WaitForElement($"#{nameof(IoTEdgeModel.Image)}");
            Assert.IsNotNull(avatar);

            var deleteAvatarBtn = cut.WaitForElement("#deleteAvatarButton");
            deleteAvatarBtn.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.IsTrue(string.IsNullOrEmpty(cut.Find($"#{nameof(IoTEdgeModel.Image)}").Attributes["src"]?.Value)));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        private IoTEdgeModel SetupLoadEdgeModel()
        {
            var edgeModel =  new IoTEdgeModel()
            {
                ModelId = this.mockEdgeModleId,
                Name = "modelTest",
                Description = "description",
                EdgeModules = new List<IoTEdgeModule>()
                {
                    new()
                    {
                        ModuleName = "module_Test",
                        Image = "image_test",
                        Version = "1.0.1"
                    }
                },
                Image = DeviceModelImageOptions.DefaultImage
            };

            _ = this.mockEdgeModelService
                .Setup(x => x.GetIoTEdgeModel(It.Is<string>(c => c.Equals(this.mockEdgeModleId, StringComparison.Ordinal))))
                .ReturnsAsync(edgeModel);

            _ = this.mockEdgeModelService
                .Setup(x => x.GetAvatar(It.Is<string>(c => c.Equals(this.mockEdgeModleId, StringComparison.Ordinal))))
                .ReturnsAsync(edgeModel.Image);

            return edgeModel;
        }

        [Test]
        public void ClickOnAddRouteShouldAddRouteOnEdgeModelData()
        {
            // Arrange

            _ = SetupLoadEdgeModel();

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));
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

            _ = SetupLoadEdgeModel();

            // Act
            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));
            var addRouteButton = cut.WaitForElement("#addRouteButton");

            addRouteButton.Click();
            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll(".deleteRouteButton").Count));

            var deleteRouteButton = cut.WaitForElement(".deleteRouteButton");

            deleteRouteButton.Click();

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(0, cut.FindAll(".deleteRouteButton").Count));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeModelDetailPage_ClickOnAddEdgeModule_ShowAwsGreengrassComponentDialog()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            _ = SetupLoadEdgeModel();

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService
                .Setup(c => c.Show<AwsGreengrassComponentDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            // Act
            cut.WaitForElement("#add-edge-module").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeModelDetailPage_ClickOnAddPublicEdgeModules_ShowAwsGreengrassPublicComponentsDialog()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            _ = SetupLoadEdgeModel();

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService
                .Setup(c => c.Show<AwsGreengrassPublicComponentsDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<EdgeModelDetailPage>(ComponentParameter.CreateParameter("ModelID", this.mockEdgeModleId));

            // Act
            cut.WaitForElement("#add-public-edge-modules").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
