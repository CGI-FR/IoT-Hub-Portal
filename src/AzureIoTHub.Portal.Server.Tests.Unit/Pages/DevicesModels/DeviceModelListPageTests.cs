// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeviceModelListPageTests : IDisposable
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
#pragma warning restore CA2213 // Disposable fields should be disposed

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        private readonly string apiBaseUrl = "/api/models";

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
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void WhenLoraFeatureEnableDeviceDetailLinkShouldContainLora()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();
            _ = cut.WaitForElements(".detail-link");

            // Act
            var link = cut.FindAll("a.detail-link");

            // Assert
            Assert.IsNotNull(link);
            foreach (var item in link)
            {
                Assert.AreEqual($"device-models/{deviceId}?isLora=true", item.GetAttribute("href"));
            }
        }

        [Test]
        public void WhenLoraFeatureDisableDeviceDetailLinkShouldNotContainLora()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            var cut = RenderComponent<DeviceModelListPage>();
            _ = cut.WaitForElements(".detail-link");

            // Act
            var link = cut.FindAll("a.detail-link");

            // Assert
            Assert.IsNotNull(link);
            foreach (var item in link)
            {
                Assert.AreEqual($"device-models/{deviceId}", item.GetAttribute("href"));
            }
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
