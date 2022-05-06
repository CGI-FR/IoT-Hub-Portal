// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
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
    public class ConcentratorDetailPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        private readonly string mockDeviceID = Guid.NewGuid().ToString();

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            this.mockHttpClient = this.testContext.Services.AddMockHttpClient();

            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);
            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudJsEvent.connect", _ => true);
            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/lorawan/concentrators/{this.mockDeviceID}")
                .RespondJson(new Concentrator());


            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            var cut = RenderComponent<ConcentratorDetailPage>(ComponentParameter.CreateParameter("DeviceID", this.mockDeviceID));
            var returnButton = cut.WaitForElement("#returnButton");

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/lorawan/concentrators", StringComparison.OrdinalIgnoreCase));
        }
    }
}
