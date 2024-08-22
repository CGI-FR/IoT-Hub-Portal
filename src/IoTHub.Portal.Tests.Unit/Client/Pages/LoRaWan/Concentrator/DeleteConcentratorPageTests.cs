// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.LoRaWan.Concentrator
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class DeleteConcentratorPageTests : BlazorUnitTest
    {
        private DialogService dialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<ILoRaWanConcentratorClientService> mockLoRaWanConcentratorsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockLoRaWanConcentratorsClientService = MockRepository.Create<ILoRaWanConcentratorClientService>();

            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanConcentratorsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task DeleteConcentratorPageShouldDeleteConcentrator()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service =>
                    service.DeleteConcentrator(It.Is<string>(s => deviceId.Equals(s, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            IDialogReference dialogReference = null;

            _ = await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<DeleteConcentratorPage>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#delete-concentrator"));

            // Act
            cut.Find("#delete-concentrator").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeFalse();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task DeleteConcentratorPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnDeletingConcentrator()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service =>
                    service.DeleteConcentrator(It.Is<string>(s => deviceId.Equals(s, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            _ = await cut.InvokeAsync(() => this.dialogService?.Show<DeleteConcentratorPage>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#delete-concentrator"));

            // Act
            cut.Find("#delete-concentrator").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task OnClickOnCancelShouldCancelDialog()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            _ = await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<DeleteConcentratorPage>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#cancel-delete-concentrator"));

            // Act
            cut.Find("#cancel-delete-concentrator").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
