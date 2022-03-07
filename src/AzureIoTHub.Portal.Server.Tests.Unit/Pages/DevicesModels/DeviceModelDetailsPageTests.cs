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
using AzureIoTHub.Portal.Shared.Models.V10;
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

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    [TestFixture]
    public class DeviceModelDetaislPageTests
    {
        private Bunit.TestContext testContext;
        private string mockModelId = Guid.NewGuid().ToString();

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;
        private MockHttpMessageHandler mockHttpClient;

        private string apiSettingsBaseUrl = "/api/settings/lora";

        private string apiBaseUrl =>  $"/api/models/{mockModelId}";

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
            testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

            mockNavigationManager = testContext.Services.GetRequiredService<FakeNavigationManager>();
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void When_Lora_Feature_Is_Disabled_Model_Details_Should_Not_Display_LoRaWAN_Tab()
        {
            // Arrange
            this.mockHttpClient.When(apiBaseUrl)
                                .RespondJson(new DeviceModel
                                {
                                    ModelId= this.mockModelId
                                });

            this.mockHttpClient.When($"{apiBaseUrl}/avatar")
                                .RespondText($"http://fake.local/{this.mockModelId}");

            this.mockHttpClient.When($"{apiBaseUrl}/properties")
                    .RespondJson(new DeviceProperty[0]);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), mockModelId));

            cut.WaitForElement("#form", 1.Seconds());

            // Assert
            var tabs = cut.FindAll(".mud-tabs .mud-tab");
            Assert.AreEqual(1, tabs.Count);
            Assert.AreEqual("General", tabs.Single().TextContent);
        }

        [Test]
        public void When_Lora_Feature_Is_Enabled_Model_Details_Should_Display_LoRaWAN_Tab()
        {
            // Arrange
            this.mockHttpClient.When(apiBaseUrl)
                                .RespondJson(new DeviceModel
                                {
                                    ModelId = this.mockModelId,
                                });

            this.mockHttpClient.When($"{apiBaseUrl}/avatar")
                                .RespondText($"http://fake.local/{this.mockModelId}");

            this.mockHttpClient.When($"{apiBaseUrl}/properties")
                    .RespondJson(new DeviceProperty[0]);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), mockModelId),
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.IsLoRa), true));

            cut.WaitForElement("#form", 1.Seconds());

            // Assert
            var tabs = cut.FindAll(".mud-tabs .mud-tab");
            Assert.AreEqual(2, tabs.Count);
            Assert.AreEqual("General", tabs[0].TextContent);
            Assert.AreEqual("LoRaWAN", tabs[1].TextContent);
        }
    }
}
