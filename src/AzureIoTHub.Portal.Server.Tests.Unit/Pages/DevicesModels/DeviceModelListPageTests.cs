using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureIoTHub.Portal.Client.Pages.DeviceModels;
using AzureIoTHub.Portal.Client.Pages.Devices;
using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
using AzureIoTHub.Portal.Shared.Models.V10.Device;
using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
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

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DevicesModels
{
    [TestFixture]
    public class DeviceModelListPageTests
    {
        private Bunit.TestContext testContext;

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;
        private MockHttpMessageHandler mockHttpClient;

        private string apiBaseUrl = "/api/models";
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
        public void When_Lora_Feature_enable_device_detail_link_Should_contain_lora()
        {
            // Arrange
            string deviceId = Guid.NewGuid().ToString();

            this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.mockHttpClient
                    .When(HttpMethod.Get, apiSettingsBaseUrl)
                    .RespondJson(true);

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
        public void When_Lora_Feature_disable_device_detail_link_Should_not_contain_lora()
        {
            // Arrange
            string deviceId = Guid.NewGuid().ToString();

            this.mockHttpClient
                .When(HttpMethod.Get, apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.mockHttpClient
                    .When(HttpMethod.Get, apiSettingsBaseUrl)
                    .RespondJson(false);

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
    }
}
