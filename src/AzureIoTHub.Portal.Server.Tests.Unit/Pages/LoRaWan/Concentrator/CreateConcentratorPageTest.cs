// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.LoRaWan.Concentrator
{
    using System;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
    using AzureIoTHub.Portal.Client.Shared;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Mocks;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateConcentratorPageTest : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;

        private FakeNavigationManager mockNavigationManager;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
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

            _ = MockHttpClient
                .When(HttpMethod.Post, $"/api/lorawan/concentrators")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<Concentrator>>(m.Content);
                    var objectContent = m.Content as ObjectContent<Concentrator>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<Concentrator>(objectContent.Value);
                    var concentrator = objectContent.Value as Concentrator;
                    Assert.IsNotNull(concentrator);

                    Assert.AreEqual(mockConcentrator.DeviceId, concentrator.DeviceId);
                    Assert.AreEqual(mockConcentrator.DeviceName, concentrator.DeviceName);
                    Assert.AreEqual(mockConcentrator.LoraRegion, concentrator.LoraRegion);

                    return true;
                })
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

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
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
            MockRepository.VerifyAll();
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

            _ = MockHttpClient
                .When(HttpMethod.Post, $"/api/lorawan/concentrators")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<Concentrator>>(m.Content);
                    var objectContent = m.Content as ObjectContent<Concentrator>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<Concentrator>(objectContent.Value);
                    var concentrator = objectContent.Value as Concentrator;
                    Assert.IsNotNull(concentrator);

                    Assert.AreEqual(mockConcentrator.DeviceId, concentrator.DeviceId);
                    Assert.AreEqual(mockConcentrator.DeviceName, concentrator.DeviceName);
                    Assert.AreEqual(mockConcentrator.LoraRegion, concentrator.LoraRegion);

                    return true;
                })
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            var cut = RenderComponent<CreateConcentratorPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));


            cut.Find($"#{nameof(Concentrator.DeviceId)}").Change(mockConcentrator.DeviceId);
            cut.Find($"#{nameof(Concentrator.DeviceName)}").Change(mockConcentrator.DeviceName);
            cut.Instance.ChangeRegion(mockConcentrator.LoraRegion);

            // Act
            cut.Find("#saveButton").Click();

            // Assert            
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldNotCreateConcentratorWhenModelIsNotValid()
        {
            // Arrange

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, null)).Returns((Snackbar)null);

            var cut = RenderComponent<CreateConcentratorPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert            
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
