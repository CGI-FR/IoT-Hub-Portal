// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Layer
{
    using Bunit;
    using FluentAssertions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Dialogs.Layer;
    using MudBlazor;
    using IoTHub.Portal.Client.Exceptions;

    internal class DeleteLayerDialogTest : BlazorUnitTest
    {
        private Mock<ILayerClientService> mockLayerClientService;
        private DialogService dialogService;

        public override void Setup()
        {
            base.Setup();

            this.mockLayerClientService = MockRepository.Create<ILayerClientService>();
            _ = Services.AddSingleton(this.mockLayerClientService.Object);

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task DeleteLayerDialog_Close()
        {
            var cut = RenderComponent<MudDialogProvider>();
            var parameters = new DialogParameters
            {
                {
                    "layerId", "Id"
                },
                {
                    "layerName", "Layer"
                },

            };

            IDialogReference dialogReference = null;

            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<DeleteLayerDialog>(string.Empty, parameters));

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().NotBe(TaskStatus.RanToCompletion);

            // Act
            cut.WaitForElement("#deleteLayerDialogCloseButton").Click();

            // Assert
            _ = dialogReference.Should().NotBeNull();

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().Be(TaskStatus.RanToCompletion);

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteLayerDialog_Submit()
        {
            _ = this.mockLayerClientService.Setup(service => service.DeleteLayer(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<MudDialogProvider>();
            var parameters = new DialogParameters
            {
                {
                    "layerId", "Id"
                },
                {
                    "layerName", "Layer"
                },

            };

            IDialogReference dialogReference = null;

            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<DeleteLayerDialog>(string.Empty, parameters));

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().NotBe(TaskStatus.RanToCompletion);

            // Act
            cut.WaitForElement("#deleteLayerDialogDeleteButton").Click();

            // Assert
            _ = dialogReference.Should().NotBeNull();

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().Be(TaskStatus.RanToCompletion);

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteLayerDialog_SubmitError()
        {
            _ = this.mockLayerClientService.Setup(service => service.DeleteLayer(It.IsAny<string>()))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();
            var parameters = new DialogParameters
            {
                {
                    "layerId", "Id"
                },
                {
                    "layerName", "Layer"
                },

            };

            IDialogReference dialogReference = null;

            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<DeleteLayerDialog>(string.Empty, parameters));

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().NotBe(TaskStatus.RanToCompletion);

            // Act
            cut.WaitForElement("#deleteLayerDialogDeleteButton").Click();

            // Assert
            _ = dialogReference.Should().NotBeNull();

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().Be(TaskStatus.WaitingForActivation);

            this.MockRepository.VerifyAll();
        }
    }
}
