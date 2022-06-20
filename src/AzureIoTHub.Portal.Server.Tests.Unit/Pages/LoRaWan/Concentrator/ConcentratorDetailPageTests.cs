// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
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
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConcentratorDetailPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        private readonly string mockDeviceID = Guid.NewGuid().ToString();

        private FakeNavigationManager mockNavigationManager;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            this.mockHttpClient = this.testContext.Services.AddMockHttpClient();

            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudJsEvent.connect", _ => true);

            this.mockHttpClient.AutoFlush = true;

            this.mockNavigationManager = this.testContext.Services.GetRequiredService<FakeNavigationManager>();
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/lorawan/concentrators/{this.mockDeviceID}")
                .RespondJson(new Concentrator());

            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", this.mockDeviceID));
            cut.WaitForAssertion(() => cut.Find("#returnButton"));

            // Act
            cut.Find("#returnButton").Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/lorawan/concentrators"));
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ConcentratorDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConcentratorDetails()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/lorawan/concentrators/{this.mockDeviceID}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", this.mockDeviceID));
            cut.WaitForAssertion(() => cut.Find("#returnButton"));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ClickOnSaveShouldPutConcentratorDetails()
        {
            // Arrange
            var mockConcentrator = new Concentrator()
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString()
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/lorawan/concentrators/{mockConcentrator.DeviceId}")
                .RespondJson(mockConcentrator);

            _ = this.mockHttpClient
                .When(HttpMethod.Put, $"/api/lorawan/concentrators")
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

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockConcentrator.DeviceId));
            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/lorawan/concentrators"));
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnSaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnUpdatingConcentratorDetails()
        {
            // Arrange
            var mockConcentrator = new Concentrator()
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = Guid.NewGuid().ToString()
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/lorawan/concentrators/{mockConcentrator.DeviceId}")
                .RespondJson(mockConcentrator);

            _ = this.mockHttpClient
                .When(HttpMethod.Put, $"/api/lorawan/concentrators")
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

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockConcentrator.DeviceId));
            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

    }
}
