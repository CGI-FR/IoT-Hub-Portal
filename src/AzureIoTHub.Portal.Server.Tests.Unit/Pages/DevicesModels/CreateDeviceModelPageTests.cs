using AzureIoTHub.Portal.Client.Pages.DeviceModels;
using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
using AzureIoTHub.Portal.Shared.Models;
using AzureIoTHub.Portal.Shared.Models.V10;
using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    [TestFixture]
    public class CreateDeviceModelPageTests
    {
        private Bunit.TestContext testContext;
        private string mockModelId = Guid.NewGuid().ToString();

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;
        private MockHttpMessageHandler mockHttpClient;

        private string apiSettingsLora = "/api/settings/lora";
        private string apiBaseUrl => $"/api/models";
        private string lorawanApiBaseUrl => $"/api/lorawan/models";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            this.mockHttpClient = testContext.Services
                                            .AddMockHttpClient();

            testContext.Services.AddSingleton(this.mockDialogService.Object);

            testContext.Services.AddMudServices();

            testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

            mockNavigationManager = testContext.Services.GetRequiredService<FakeNavigationManager>();

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void Click_On_Save_Should_Post_Device_Model_Data()
        {
            // Arrange
            var modelName = Guid.NewGuid().ToString();
            var description = Guid.NewGuid().ToString();

            var expectedProperties = Enumerable.Range(0, 1)
                .Select(x => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Double
                }).ToArray();

            this.mockHttpClient.When(this.apiSettingsLora)
                .RespondJson(false);

            this.mockHttpClient.When(HttpMethod.Post, $"{apiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<DeviceModel>(jsonContent.Value);
                    var deviceModel = jsonContent.Value as DeviceModel;

                    Assert.IsNotNull(deviceModel.ModelId);
                    Assert.AreEqual(deviceModel.Name, modelName);
                    Assert.AreEqual(deviceModel.Description, description);
                    Assert.AreEqual(deviceModel.SupportLoRaFeatures, false);

                    return true;
                })
                .RespondText(string.Empty);

            this.mockHttpClient
                .When(HttpMethod.Post, $"{apiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(expectedProperties.Count(), properties.Count());

                    foreach (var expectedProperty in expectedProperties)
                    {
                        var property = properties.Single(x => x.Name == expectedProperty.Name);

                        Assert.AreEqual(expectedProperty.Name, property.Name);
                        Assert.AreEqual(expectedProperty.DisplayName, property.DisplayName);
                        Assert.AreEqual(expectedProperty.PropertyType, property.PropertyType);
                    }

                    return true;
                })
                .RespondText(string.Empty);

            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            // Act
            cut.Find($"#{nameof(DeviceModel.Name)}").Change(modelName);
            cut.Find($"#{nameof(DeviceModel.Description)}").Change(description);

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models"));

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void Click_On_Add_Property_Should_Add_NewProperty()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            this.mockHttpClient.When(HttpMethod.Post, $"{ apiBaseUrl}")
                .RespondText(string.Empty);

            this.mockHttpClient.When(this.apiSettingsLora)
                .RespondJson(false);

            this.mockHttpClient.When(HttpMethod.Post, $"{ apiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(1, properties.Count());

                    var property = properties.Single(x => x.Name == propertyName);

                    Assert.AreEqual(propertyName, property.Name);
                    Assert.AreEqual(displayName, property.DisplayName);
                    Assert.AreEqual(DevicePropertyType.Boolean, property.PropertyType);
                    Assert.IsTrue(property.IsWritable);

                    return true;
                })
                .RespondText(string.Empty);

            var cut = RenderComponent<CreateDeviceModelPage>();

            var saveButton = cut.WaitForElement("#SaveButton");
            var addPropertyButton = cut.WaitForElement("#addPropertyButton");

            cut.Find($"#{nameof(DeviceModel.Name)}").Change(Guid.NewGuid().ToString());
            cut.Find($"#{nameof(DeviceModel.Description)}").Change(Guid.NewGuid().ToString());

            // Act
            addPropertyButton.Click();

            cut.WaitForElement($"#property- #{nameof(DeviceProperty.Name)}").Change(propertyName);

            var propertyCssSelector = $"#property-{propertyName}";

            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.DisplayName)}").Change(displayName);
            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.PropertyType)}").Change(DevicePropertyType.Boolean.ToString());
            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.IsWritable)}").Change(true);

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models"));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void Click_On_Remove_Property_Should_Remove_The_Property()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            this.mockHttpClient.When(this.apiSettingsLora)
                .RespondJson(false);

            this.mockHttpClient.When(HttpMethod.Post, $"{ apiBaseUrl}")
                .RespondText(string.Empty);

            this.mockHttpClient.When(HttpMethod.Post, $"{ apiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(0, properties.Count());

                    return true;
                })
                .RespondText(string.Empty);

            var cut = RenderComponent<CreateDeviceModelPage>();

            var saveButton = cut.WaitForElement("#SaveButton");

            cut.Find($"#{nameof(DeviceModel.Name)}").Change(Guid.NewGuid().ToString());
            cut.Find($"#{nameof(DeviceModel.Description)}").Change(Guid.NewGuid().ToString());

            var addPropertyButton = cut.WaitForElement("#addPropertyButton");
            addPropertyButton.Click();

            var removePropertyButton = cut.WaitForElement("#DeletePropertyButton");

            // Act
            removePropertyButton.Click();

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models"));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void When_Lora_Feature_Is_Disabled_Model_Details_Should_Not_Display_LoRaWAN_Switch()
        {
            // Arrange
            this.mockHttpClient.When(this.apiSettingsLora)
                .RespondJson(false);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            cut.WaitForElement("#form");


            // Assert
            Assert.AreEqual(0, cut.FindAll("#SupportLoRaFeatures").Count());
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void When_Lora_Feature_Is_Enabled_Model_Details_Should_Display_LoRaWAN_Switch()
        {
            // Arrange
            this.mockHttpClient.When(this.apiSettingsLora)
                .RespondJson(true);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            cut.WaitForElement("#form");
            cut.WaitForElement("#SupportLoRaFeatures");

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void When_Lora_Feature_Is_Enabled_Model_Details_Should_Display_LoRaWAN_Tab()
        {
            // Arrange
            this.mockHttpClient.When(this.apiSettingsLora)
                .RespondJson(true);

            var cut = RenderComponent<CreateDeviceModelPage>();
            cut.WaitForElement("#form");

            // Act
            cut.WaitForElement("#SupportLoRaFeatures")
                .Change(true);

            cut.WaitForState(() => cut.FindAll(".mud-tabs .mud-tab").Count() == 2);
            // Assert
            var tabs = cut.FindAll(".mud-tabs .mud-tab");
            Assert.AreEqual(2, tabs.Count);
            Assert.AreEqual("General", tabs[0].TextContent);
            Assert.AreEqual("LoRaWAN", tabs[1].TextContent);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
