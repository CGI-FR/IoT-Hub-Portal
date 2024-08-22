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
    public class ConcentratorDetailPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ILoRaWanConcentratorClientService> mockLoRaWanConcentratorsClientService;

        private readonly string mockDeviceId = Guid.NewGuid().ToString();

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockLoRaWanConcentratorsClientService = MockRepository.Create<ILoRaWanConcentratorClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanConcentratorsClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetConcentrator(this.mockDeviceId))
                .ReturnsAsync(new ConcentratorDto());

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetFrequencyPlans())
                .ReturnsAsync(new[]
                {
                    new FrequencyPlan()
                });

            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", this.mockDeviceId));
            cut.WaitForAssertion(() => cut.Find("#returnButton"));

            // Act
            cut.Find("#returnButton").Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/lorawan/concentrators"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ConcentratorDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConcentratorDetails()
        {
            // Arrange
            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetConcentrator(this.mockDeviceId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetFrequencyPlans())
                .ReturnsAsync(new[]
                {
                    new FrequencyPlan()
                });

            // Act
            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", this.mockDeviceId));
            cut.WaitForAssertion(() => cut.Find("#returnButton"));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldPutConcentratorDetails()
        {
            // Arrange
            var mockConcentrator = new ConcentratorDto()
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString()
            };

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetFrequencyPlans())
                .ReturnsAsync(new[]
                {
                    new FrequencyPlan()
                });

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetConcentrator(mockConcentrator.DeviceId))
                .ReturnsAsync(mockConcentrator);

            _ = this.mockLoRaWanConcentratorsClientService
                .Setup(service => service.UpdateConcentrator(mockConcentrator))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockConcentrator.DeviceId));
            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/lorawan/concentrators"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ConcentratorShouldNotBeUpdatedWhenModelIsNotValid()
        {
            // Arrange
            var mockConcentrator = new ConcentratorDto()
            {
                DeviceId = string.Empty,
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString()
            };

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetFrequencyPlans())
                .ReturnsAsync(new[]
                {
                                new FrequencyPlan()
                });

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetConcentrator(mockConcentrator.DeviceId))
                .ReturnsAsync(mockConcentrator);

            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockConcentrator.DeviceId));
            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnUpdatingConcentratorDetails()
        {
            // Arrange
            var mockConcentrator = new ConcentratorDto()
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString()
            };

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetFrequencyPlans())
                .ReturnsAsync(new[]
                {
                    new FrequencyPlan()
                });

            _ = this.mockLoRaWanConcentratorsClientService.Setup(service => service.GetConcentrator(mockConcentrator.DeviceId))
                .ReturnsAsync(mockConcentrator);

            _ = this.mockLoRaWanConcentratorsClientService
                .Setup(service => service.UpdateConcentrator(mockConcentrator))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockConcentrator.DeviceId));
            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
