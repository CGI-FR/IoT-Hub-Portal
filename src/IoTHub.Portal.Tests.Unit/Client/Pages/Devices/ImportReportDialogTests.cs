// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Dialogs.Devices;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class ImportReportDialogTests : BlazorUnitTest
    {
        private Mock<IDeviceClientService> mockDeviceClientService;
        private DialogService dialogService;

        public override void Setup()
        {
            base.Setup();

            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();

            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task ImportShouldDisplayLoadingDuringProcess()
        {
            // Arrange
            var shouldWait = true;

            _ = this.mockDeviceClientService
                .Setup(c => c.ImportDeviceList(It.IsAny<MultipartFormDataContent>()))
                .Returns(async (MultipartFormDataContent _) =>
                {
                    while (shouldWait)
                        await Task.Delay(100);

                    return Array.Empty<ImportResultLine>();
                });

            var cut = RenderComponent<MudDialogProvider>();
            var parameters = new DialogParameters
            {
                {
                    "Content", null
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => this.dialogService?.Show<ImportReportDialog>(string.Empty, parameters));

            // Assert
            cut.WaitForState(() => cut.HasComponent<MudProgressCircular>());
            shouldWait = false;
            cut.WaitForState(() => !cut.HasComponent<MudProgressCircular>());

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenNoErrorsDuringImportShouldDisplaySuccessMessage()
        {
            // Arrange
            _ = this.mockDeviceClientService
                .Setup(c => c.ImportDeviceList(It.IsAny<MultipartFormDataContent>()))
                .ReturnsAsync(Array.Empty<ImportResultLine>());

            var cut = RenderComponent<MudDialogProvider>();
            var parameters = new DialogParameters
            {
                {
                    "Content", null
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => this.dialogService?.Show<ImportReportDialog>(string.Empty, parameters));
            cut.WaitForState(() => !cut.HasComponent<MudProgressCircular>());

            // Assert
            var alerts = cut.FindComponents<MudAlert>();

            _ = alerts.Should().HaveCount(1);
            _ = alerts[0].Instance.Severity.Should().Be(Severity.Success);
            _ = alerts[0].Markup.Should().Contain("Devices have been imported successfully!");

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenExceptionOccuresShouldCloseThePopup()
        {
            // Arrange
            _ = this.mockDeviceClientService
                .Setup(c => c.ImportDeviceList(It.IsAny<MultipartFormDataContent>()))
                .Throws(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();
            var parameters = new DialogParameters
            {
                {
                    "Content", null
                }
            };

            // Act
            IDialogReference dialogReference = null;

            _ = await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<ImportReportDialog>(string.Empty, parameters));

            // Assert
            _ = dialogReference.Should().NotBeNull();

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().Be(TaskStatus.RanToCompletion);

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenErrorsDuringImportShouldDisplayErrorMessages()
        {
            // Arrange

            _ = this.mockDeviceClientService
                .Setup(c => c.ImportDeviceList(It.IsAny<MultipartFormDataContent>()))
                .ReturnsAsync(new[]
                {
                    new ImportResultLine
                    {
                        IsErrorMessage = true,
                        DeviceId = "0",
                        LineNumber = 10,
                        Message = "Toto"
                    },
                    new ImportResultLine
                    {
                        IsErrorMessage = false,
                        DeviceId = "0",
                        LineNumber = 10,
                        Message = "Toto"
                    }
                });

            var cut = RenderComponent<MudDialogProvider>();
            var parameters = new DialogParameters
            {
                {
                    "Content", null
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => this.dialogService?.Show<ImportReportDialog>(string.Empty, parameters));
            cut.WaitForState(() => !cut.HasComponent<MudProgressCircular>());

            // Assert
            var alerts = cut.FindComponents<MudAlert>();

            _ = alerts.Should().HaveCount(3);

            _ = alerts.Where(c => c.Instance.Severity == Severity.Error)
                .Should()
                .ContainSingle()
                .Subject.Markup.Should().Contain("<b>Error occured</b> on <b>line {10}</b>: Toto");


            _ = alerts.Where(c => c.Instance.Severity == Severity.Info)
                .Should()
                .ContainSingle()
                .Subject.Markup.Should().Contain("<b>Info</b> on <b>line {10}</b>: Toto");


            _ = alerts.Where(c => c.Instance.Severity == Severity.Success)
                .Should()
                .ContainSingle()
                .Subject.Markup.Should().Contain("Other devices have been imported successfully!");

            this.MockRepository.VerifyAll();
        }

        [Test]
        public async Task ClickOnCloseShouldCloseTheDialog()
        {
            // Arrange

            _ = this.mockDeviceClientService
                .Setup(c => c.ImportDeviceList(It.IsAny<MultipartFormDataContent>()))
                .ReturnsAsync(Array.Empty<ImportResultLine>());

            var cut = RenderComponent<MudDialogProvider>();
            var parameters = new DialogParameters
            {
                {
                    "Content", null
                }
            };

            IDialogReference dialogReference = null;

            _ = await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<ImportReportDialog>(string.Empty, parameters));

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().NotBe(TaskStatus.RanToCompletion);

            // Act
            cut.WaitForElement("#CloseButton").Click();

            // Assert
            _ = dialogReference.Should().NotBeNull();

            _ = dialogReference.Result.Should().NotBeNull()
                .And.BeOfType<Task<DialogResult>>()
                .Subject.Status.Should().Be(TaskStatus.RanToCompletion);

            this.MockRepository.VerifyAll();
        }
    }
}
