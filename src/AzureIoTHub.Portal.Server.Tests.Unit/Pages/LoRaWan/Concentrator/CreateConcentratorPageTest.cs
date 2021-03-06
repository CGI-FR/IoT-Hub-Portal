// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.LoRaWan.Concentrator
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Mocks;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class CreateConcentratorPageTest : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<ILoRaWanConcentratorsClientService> mockLoRaWanConcentratorsClientService;

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockLoRaWanConcentratorsClientService = MockRepository.Create<ILoRaWanConcentratorsClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanConcentratorsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void ClickOnSaveShouldPostConcentratorDetails()
        {
            // Arrange
            var mockConcentrator = new Concentrator
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = "CN_470_510_RP2"
            };

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service =>
                    service.CreateConcentrator(It.Is<Concentrator>(concentrator =>
                        mockConcentrator.DeviceId.Equals(concentrator.DeviceId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);

            var cut = RenderComponent<CreateConcentratorPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));


            cut.Find($"#{nameof(Concentrator.DeviceId)}").Change(mockConcentrator.DeviceId);
            cut.Find($"#{nameof(Concentrator.DeviceName)}").Change(mockConcentrator.DeviceName);
            cut.Instance.ChangeRegion(mockConcentrator.LoraRegion);

            // Act
            cut.Find("#saveButton").Click();

            // Assert            
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/lorawan/concentrators"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnCreatingConcentratorDetails()
        {
            // Arrange
            var mockConcentrator = new Concentrator
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = "CN_470_510_RP2"
            };

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service =>
                    service.CreateConcentrator(It.Is<Concentrator>(concentrator =>
                        mockConcentrator.DeviceId.Equals(concentrator.DeviceId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<CreateConcentratorPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));


            cut.Find($"#{nameof(Concentrator.DeviceId)}").Change(mockConcentrator.DeviceId);
            cut.Find($"#{nameof(Concentrator.DeviceName)}").Change(mockConcentrator.DeviceName);
            cut.Instance.ChangeRegion(mockConcentrator.LoraRegion);

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldNotCreateConcentratorWhenModelIsNotValid()
        {
            // Arrange
            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, null)).Returns((Snackbar)null);

            var cut = RenderComponent<CreateConcentratorPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
