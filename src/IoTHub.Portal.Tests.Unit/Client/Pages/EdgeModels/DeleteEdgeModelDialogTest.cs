// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.EdgeModels
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Dialogs.EdgeModels;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    public class DeleteEdgeModelDialogTest : BlazorUnitTest
    {
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IEdgeModelClientService> mockEdgeModelClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockEdgeModelClientService = MockRepository.Create<IEdgeModelClientService>();

            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockEdgeModelClientService.Object);
        }

        [Test]
        public async Task ClickOnDeleteButtonShouldCloseDialog()
        {
            // Arrange
            var edgeModelId = Guid.NewGuid().ToString();
            var modelName = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelClientService
                .Setup(x => x.DeleteIoTEdgeModel(It.Is<string>(c => c.Equals(edgeModelId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService
                .Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns(value: null);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceModelID", edgeModelId
                },
                {
                    "deviceModelName", modelName
                }
            };

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<DeleteEdgeModelDialog>(string.Empty, parameters));

            var deleteBtn = cut.Find("#deleteButton");
            deleteBtn.Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Data.Should().Be(true);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ClickOnDeleteButtonShouldProssessProblemDetailsException()
        {
            // Arrange
            var edgeModelId = Guid.NewGuid().ToString();
            var modelName = Guid.NewGuid().ToString();

            _ = this.mockEdgeModelClientService
                .Setup(x => x.DeleteIoTEdgeModel(It.Is<string>(c => c.Equals(edgeModelId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceModelID", edgeModelId
                },
                {
                    "deviceModelName", modelName
                }
            };

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<DeleteEdgeModelDialog>(string.Empty, parameters));

            var deleteBtn = cut.Find("#deleteButton");
            deleteBtn.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task ClickOnCancelButtonShouldCloseDialog()
        {
            // Arrange
            var edgeModelId = Guid.NewGuid().ToString();
            var modelName = Guid.NewGuid().ToString();

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceModelID", edgeModelId
                },
                {
                    "deviceModelName", modelName
                }
            };

            IDialogReference dialogReference = null;

            // Act
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<DeleteEdgeModelDialog>(string.Empty, parameters));

            var deleteBtn = cut.Find("#cancelButton");
            deleteBtn.Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
