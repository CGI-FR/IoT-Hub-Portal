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
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;
    using Portal.Shared.Models.v1._0.LoRaWAN;
    using UnitTests.Mocks;

    [TestFixture]
    public class CreateConcentratorPageTest : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<ILoRaWanConcentratorClientService> mockLoRaWanConcentratorsClientService;

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockLoRaWanConcentratorsClientService = MockRepository.Create<ILoRaWanConcentratorClientService>();

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
            var mockConcentrator = new ConcentratorDto
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = "CN_470_510_RP2"
            };

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetFrequencyPlans())
                .ReturnsAsync(new[]
                {
                    new FrequencyPlan()
                });

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service =>
                    service.CreateConcentrator(It.Is<ConcentratorDto>(concentrator =>
                        mockConcentrator.DeviceId.Equals(concentrator.DeviceId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            var cut = RenderComponent<CreateConcentratorPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));


            cut.Find($"#{nameof(ConcentratorDto.DeviceId)}").Change(mockConcentrator.DeviceId);
            cut.Find($"#{nameof(ConcentratorDto.DeviceName)}").Change(mockConcentrator.DeviceName);
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
            var mockConcentrator = new ConcentratorDto
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = "CN_470_510_RP2"
            };

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetFrequencyPlans())
                .ReturnsAsync(new[]
                {
                    new FrequencyPlan()
                });

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service =>
                    service.CreateConcentrator(It.Is<ConcentratorDto>(concentrator =>
                        mockConcentrator.DeviceId.Equals(concentrator.DeviceId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<CreateConcentratorPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));


            cut.Find($"#{nameof(ConcentratorDto.DeviceId)}").Change(mockConcentrator.DeviceId);
            cut.Find($"#{nameof(ConcentratorDto.DeviceName)}").Change(mockConcentrator.DeviceName);
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
            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);
            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetFrequencyPlans())
                .ReturnsAsync(new[]
                {
                    new FrequencyPlan()
                });

            var cut = RenderComponent<CreateConcentratorPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
