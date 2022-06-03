// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.LoRaWan.Concentrator
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using AzureIoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
    using AzureIoTHub.Portal.Client.Shared;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateConcentratorPageTest : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;

        private FakeNavigationManager mockNavigationManager;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = this.testContext.Services.AddMockHttpClient();

            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);

            this.mockSnackbarService = this.mockRepository.Create<ISnackbar>();
            _ = this.testContext.Services.AddSingleton(this.mockSnackbarService.Object);

            _ = this.testContext.Services.AddMudServices();
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudJsEvent.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.updatekey", _ => true);
            this.mockHttpClient.AutoFlush = true;

            this.mockNavigationManager = this.testContext.Services.GetRequiredService<FakeNavigationManager>();
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void ClickOnSaveShouldPostConcentratorDetails()
        {
            var mockConcentrator = new Concentrator()
            {
                DeviceId = "1234567890123456",
                DeviceName = Guid.NewGuid().ToString(),
                LoraRegion = "CN_470_510_RP2"
            };

            _ = this.mockHttpClient
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

            var cut = RenderComponent<CreateConcentratorPage>();
            var saveButton = cut.WaitForElement("#saveButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);


            cut.Find($"#{nameof(Concentrator.DeviceId)}").Change(mockConcentrator.DeviceId);
            cut.Find($"#{nameof(Concentrator.DeviceName)}").Change(mockConcentrator.DeviceName);
            cut.Instance.ChangeRegion(mockConcentrator.LoraRegion);

            saveButton.Click();
            Thread.Sleep(1000);

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/lorawan/concentrators", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void ClickOnSaveShouldDisplayErrorSnackbarIfValidationError()
        {
            var cut = RenderComponent<CreateConcentratorPage>();
            var saveButton = cut.WaitForElement("#saveButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, null)).Returns((Snackbar)null);

            saveButton.Click();
            Thread.Sleep(1000);

            // Assert            
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
