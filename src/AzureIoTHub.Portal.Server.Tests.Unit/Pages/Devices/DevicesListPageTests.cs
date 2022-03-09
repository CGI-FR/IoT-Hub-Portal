// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions.Extensions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;


    [TestFixture]
    public class DevicesListPageTests : IDisposable
    {
        private Bunit.TestContext testContext;

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;
        private MockHttpMessageHandler mockHttpClient;

        private readonly string apiBaseUrl = "/api/Devices";
        private readonly string apiSettingsBaseUrl = "/api/settings/lora";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            this.mockHttpClient = testContext.Services.AddMockHttpClient();

            _ = testContext.Services.AddSingleton(this.mockDialogService.Object);

            _ = testContext.Services.AddMudServices();

            _ = testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);

            mockNavigationManager = testContext.Services.GetRequiredService<FakeNavigationManager>();
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void DeviceListPageRendersCorrectly()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(Array.Empty<object>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiSettingsBaseUrl)
                .RespondJson(true);

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.Find("h5")
                .MarkupMatches("<h5 class=\"mud-typography mud-typography-h5 mud-primary-text mb-4\">Device List</h5>");

            Assert.AreEqual("Search panel", cut.Find(".mud-expansion-panels .mud-expand-panel .mud-expand-panel-header .mud-expand-panel-text").TextContent);

            Assert.IsFalse(cut.Find(".mud-expansion-panels .mud-expand-panel")
                                .ClassList.Contains("mud-panel-expanded"), "Search panel should be collapsed");

            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenDevicesNotLoadedDeviceListPageShouldRenderProgressBar()
        {
            // Arrange
            var task = Task.Factory.StartNew(() =>
            {
                while (true) { }

                return Array.Empty<object>();
            });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJsonAsync(task);

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiSettingsBaseUrl)
                .RespondJson(true);

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.Find("#progress-bar")
                 .MarkupMatches("<div id=\"progress-bar\" class=\"mud-grid-item custom-centered-container\"><!--!--><div class=\"mud-progress-circular mud-default-text mud-progress-medium mud-progress-indeterminate custom-centered-item\" role=\"progressbar\" aria-valuenow=\"0\"><svg class=\"mud-progress-circular-svg\" viewBox=\"22 22 44 44\"><circle class=\"mud-progress-circular-circle mud-progress-indeterminate\" cx=\"44\" cy=\"44\" r=\"20\" fill=\"none\" stroke-width=\"3\"></circle></svg></div><!--!--></div>");

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenResetFilterButtonClickShouldClearFilters()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(Array.Empty<object>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiSettingsBaseUrl)
                .RespondJson(true);

            var cut = RenderComponent<DeviceListPage>();

            cut.Find("#searchID").NodeValue = Guid.NewGuid().ToString();
            cut.Find("#searchStatusEnabled").Click();
            cut.Find("#searchStateDisconnected").Click();

            // Act
            cut.Find("#resetSearch").Click();
            await Task.Delay(100);

            // Assert
            Assert.IsNull(cut.Find("#searchID").NodeValue);
            Assert.AreEqual("false", cut.Find("#searchStatusEnabled").Attributes["aria-checked"].Value);
            Assert.AreEqual("false", cut.Find("#searchStateDisconnected").Attributes["aria-checked"].Value);
            Assert.AreEqual("true", cut.Find("#searchStatusAll").Attributes["aria-checked"].Value);
            Assert.AreEqual("true", cut.Find("#searchStateAll").Attributes["aria-checked"].Value);

            this.mockRepository.VerifyAll();
        }

        [TestCase("newDeviceButton")]
        [TestCase("tableAddItemButton")]
        public async Task WhenAddNewDeviceClickShouldNavigateToNewDevicePage(string buttonName)
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(Array.Empty<object>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiSettingsBaseUrl)
                .RespondJson(true);

            var cut = RenderComponent<DeviceListPage>();
            await Task.Delay(100);

            // Act
            cut.Find($"#{buttonName}")
                .Click();
            await Task.Delay(100);

            // Assert
            Assert.AreEqual("http://localhost/devices/new", this.mockNavigationManager.Uri);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenRefreshClickShouldReloadFromApi()
        {
            // Arrange
            var apiCall = this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(Array.Empty<object>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiSettingsBaseUrl)
                .RespondJson(true);

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Find($"#tableRefreshButton"), 1.Seconds());

            // Act
            for (var i = 0; i < 3; i++)
            {
                cut.Find($"#tableRefreshButton")
                        .Click();
                await Task.Delay(100);
            }

            // Assert
            var matchCount = this.mockHttpClient.GetMatchCount(apiCall);
            Assert.AreEqual(4, matchCount);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenLoraFeatureDisableDeviceDetailLinkShouldNotContainLora()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(new DeviceListItem[] { new DeviceListItem { DeviceID = deviceId } });

            _ = this.mockHttpClient
                    .When(HttpMethod.Get, apiSettingsBaseUrl)
                    .RespondJson(false);

            var cut = RenderComponent<DeviceListPage>();
            _ = cut.WaitForElements(".detail-link");

            // Act
            var link = cut.FindAll("a.detail-link");

            // Assert
            Assert.IsNotNull(link);
            foreach (var item in link)
            {
                Assert.AreEqual($"devices/{deviceId}", item.GetAttribute("href"));
            }
        }

        [Test]
        public void WhenLoraFeatureEnableDeviceDetailLinkShouldContainLora()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(new DeviceListItem[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true } });

            _ = this.mockHttpClient
                    .When(HttpMethod.Get, apiSettingsBaseUrl)
                    .RespondJson(true);

            var cut = RenderComponent<DeviceListPage>();
            _ = cut.WaitForElements(".detail-link");

            // Act
            var link = cut.FindAll("a.detail-link");

            // Assert
            Assert.IsNotNull(link);
            foreach (var item in link)
            {
                Assert.AreEqual($"devices/{deviceId}?isLora=true", item.GetAttribute("href"));
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
