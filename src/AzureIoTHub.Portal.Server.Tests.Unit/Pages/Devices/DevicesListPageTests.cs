using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
using Moq.Protected;
using MudBlazor;
using MudBlazor.Interop;
using MudBlazor.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace AzureIoTHub.Portal.Server.Tests.Pages
{
    [TestFixture]
    public class DevicesListPageTests
    {
        private Bunit.TestContext testContext;

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;
        private MockHttpMessageHandler mockHttpClient;

        private string apiBaseUrl = "/api/Devices";
        private string apiSettingsBaseUrl = "/api/settings/lora";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            this.mockHttpClient = testContext.Services.AddMockHttpClient();

            testContext.Services.AddSingleton(this.mockDialogService.Object);

            testContext.Services.AddMudServices();

            testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);

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

            this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(new object[0]);

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
        public void When_Devices_Not_Loaded_DeviceListPage_Should_Render_ProgressBar()
        {
            // Arrange
            var task = Task.Factory.StartNew(() =>
            {
                while (true) { }

                return new object[0];
            });

            this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJsonAsync(task);

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.Find("#progress-bar")
                 .MarkupMatches("<div id=\"progress-bar\" class=\"mud-grid-item custom-centered-container\"><!--!--><div class=\"mud-progress-circular mud-default-text mud-progress-medium mud-progress-indeterminate custom-centered-item\" role=\"progressbar\" aria-valuenow=\"0\"><svg class=\"mud-progress-circular-svg\" viewBox=\"22 22 44 44\"><circle class=\"mud-progress-circular-circle mud-progress-indeterminate\" cx=\"44\" cy=\"44\" r=\"20\" fill=\"none\" stroke-width=\"3\"></circle></svg></div><!--!--></div>");

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task When_ResetFilterButton_Click_Should_Clear_Filters()
        {
            // Arrange
            this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(new object[0]);

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
        public async Task When_AddNewDevice_Click_Should_Navigate_To_New_Device_Page(string buttonName)
        {
            // Arrange
            this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(new object[0]);

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
        public async Task When_Refresh_Click_Should_Reload_From_Api()
        {
            // Arrange
            var apiCall = this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(new object[0]);

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Find($"#tableRefreshButton"), 1.Seconds());

            // Act
            for (int i = 0; i < 3; i++)
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
        public void When_Lora_Feature_disable_device_detail_link_Should_not_contain_lora()
        {
            // Arrange
            string deviceId = Guid.NewGuid().ToString();

            this.mockHttpClient
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
        public void When_Lora_Feature_enable_device_detail_link_Should_contain_lora()
        {
            // Arrange
            string deviceId = Guid.NewGuid().ToString();

            this.mockHttpClient
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
    }
}
