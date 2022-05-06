// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Models.v10;
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
    public class DevicesDetailPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
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
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

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
            var deviceId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();


            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/devices/{deviceId}")
                .RespondJson(new DeviceDetails() { ModelId = modelId });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/devices/{deviceId}/properties")
                .RespondJson(new List<DevicePropertyValue>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>());

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", deviceId));
            var returnButton = cut.WaitForElement("#returnButton");

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/devices", StringComparison.OrdinalIgnoreCase));
        }
    }
}
