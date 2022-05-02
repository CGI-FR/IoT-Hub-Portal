// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using AzureIoTHub.Portal.Models;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
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
    using AzureIoTHub.Portal.Client.Shared;

    [TestFixture]
    public class DeviceModelDetaislPageTests : IDisposable
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
#pragma warning restore CA2213 // Disposable fields should be disposed

        private readonly string mockModelId = Guid.NewGuid().ToString();

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;

        private string ApiBaseUrl => $"/api/models/{this.mockModelId}";
        private string LorawanApiBaseUrl => $"/api/lorawan/models/{this.mockModelId}";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            this.mockHttpClient = this.testContext.Services
                                            .AddMockHttpClient();

            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);

            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

            this.mockNavigationManager = this.testContext.Services.GetRequiredService<FakeNavigationManager>();

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void ClickOnSaveShouldPostDeviceModelData()
        {
            // Arrange
            var expectedProperties = Enumerable.Range(0, 2)
                .Select(_ => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Double
                }).ToArray();

            var expectedModel = SetupMockDeviceModel(properties: expectedProperties);

            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<DeviceModel>(jsonContent.Value);
                    var deviceModel = jsonContent.Value as DeviceModel;

                    Assert.AreEqual(expectedModel.ModelId, deviceModel.ModelId);
                    Assert.AreEqual(expectedModel.Name, deviceModel.Name);
                    Assert.AreEqual(expectedModel.Description, deviceModel.Description);
                    Assert.AreEqual(expectedModel.SupportLoRaFeatures, false);
                    Assert.AreEqual(expectedModel.IsBuiltin, false);

                    return true;
                })
                .RespondText(string.Empty);

            _ = this.mockHttpClient
                .When(HttpMethod.Post, $"{ ApiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(expectedProperties.Length, properties?.Count());

                    foreach (var expectedProperty in expectedProperties)
                    {
                        var property = properties?.Single(x => x.Name == expectedProperty.Name);

                        Assert.AreEqual(expectedProperty.Name, property.Name);
                        Assert.AreEqual(expectedProperty.DisplayName, property.DisplayName);
                        Assert.AreEqual(expectedProperty.PropertyType, property.PropertyType);
                    }

                    return true;
                })
                .RespondText(string.Empty);

            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var saveButton = cut.WaitForElement("#SaveButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ClickOnAddPropertyShouldAddNewProperty()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ ApiBaseUrl }")
                .RespondJson(new DeviceModel
                {
                    ModelId = this.mockModelId,
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ ApiBaseUrl }/avatar")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ ApiBaseUrl }/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ ApiBaseUrl }")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ ApiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var jsonContent = m.Content as ObjectContent<List<DeviceProperty>>;

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

            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var saveButton = cut.WaitForElement("#SaveButton");
            var addPropertyButton = cut.WaitForElement("#addPropertyButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            addPropertyButton.Click();

            cut.WaitForElement($"#property- #{nameof(DeviceProperty.Name)}").Change(propertyName);

            var propertyCssSelector = $"#property-{propertyName}";

            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.DisplayName)}").Change(displayName);
            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.PropertyType)}").Change(nameof(DevicePropertyType.Boolean));
            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.IsWritable)}").Change(true);

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ClickOnRemovePropertyShouldRemoveTheProperty()
        {
            // Arrange
            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ ApiBaseUrl }")
                .RespondJson(new DeviceModel
                {
                    ModelId = this.mockModelId,
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ ApiBaseUrl }/avatar")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ ApiBaseUrl }/properties")
                .RespondJson(new DeviceProperty[]
                {
                    new DeviceProperty()
                });

            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ ApiBaseUrl}")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ ApiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var jsonContent = m.Content as ObjectContent<List<DeviceProperty>>;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(0, properties.Count());

                    return true;
                })
                .RespondText(string.Empty);

            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var saveButton = cut.WaitForElement("#SaveButton");
            var removePropertyButton = cut.WaitForElement("#DeletePropertyButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            removePropertyButton.Click();

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void WhenPresentModelDetailsShouldDisplayProperties()
        {
            // Arrange
            var properties = Enumerable.Range(0, 10)
                .Select(_ => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Double
                }).ToArray();

            _ = SetupMockDeviceModel(properties: properties);

            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            // Act
            _ = cut.WaitForElement("#form", 1.Seconds());

            // Assert
            foreach (var item in properties)
            {
                var propertyCssSelector = $"#property-{item.Name}";

                _ = cut.Find(propertyCssSelector);
                Assert.AreEqual(item.DisplayName, cut.Find($"{propertyCssSelector} #{nameof(item.DisplayName)}").Attributes["value"].Value);
                Assert.AreEqual(item.Name, cut.Find($"{propertyCssSelector} #{nameof(item.Name)}").Attributes["value"].Value);
                Assert.AreEqual(item.PropertyType.ToString(), cut.Find($"{propertyCssSelector} #{nameof(item.PropertyType)}").Attributes["value"].Value);
#pragma warning disable CA1308 // Normalize strings to uppercase
                Assert.AreEqual(item.IsWritable.ToString().ToLowerInvariant(), cut.Find($"{propertyCssSelector} #{nameof(item.IsWritable)}").Attributes["aria-checked"].Value);
#pragma warning restore CA1308 // Normalize strings to uppercase
            }

            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void WhenLoraFeatureIsDisabledModelDetailsShouldNotDisplayLoRaWANTab()
        {
            // Arrange
            _ = SetupMockDeviceModel();

            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            // Act
            _ = cut.WaitForElement("#form", 1.Seconds());

            // Assert
            var tabs = cut.FindAll(".mud-tabs .mud-tab");
            Assert.AreEqual(1, tabs.Count);
            Assert.AreEqual("General", tabs.Single().TextContent);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void WhenLoraFeatureIsEnabledModelDetailsShouldDisplayLoRaWANTab()
        {
            // Arrange
            _ = SetupMockLoRaWANDeviceModel();

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>(
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId),
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.IsLoRa), true));

            _ = cut.WaitForElement("#form");

            // Assert
            var tabs = cut.FindAll(".mud-tabs .mud-tab");
            Assert.AreEqual(2, tabs.Count);
            Assert.AreEqual("General", tabs[0].TextContent);
            Assert.AreEqual("LoRaWAN", tabs[1].TextContent);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        private DeviceModel SetupMockDeviceModel(DeviceProperty[] properties = null)
        {
            var deviceModel = new DeviceModel
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = new Uri($"http://fake.local/{this.mockModelId}"),
                SupportLoRaFeatures = false
            };

            _ = this.mockHttpClient.When(ApiBaseUrl)
                                .RespondJson(deviceModel);

            _ = this.mockHttpClient.When($"{ApiBaseUrl}/avatar")
                    .RespondText($"http://fake.local/{this.mockModelId}");

            _ = this.mockHttpClient.When($"{ApiBaseUrl}/properties")
                .RespondJson(properties ?? Array.Empty<DeviceProperty>());

            return deviceModel;
        }

        private LoRaDeviceModel SetupMockLoRaWANDeviceModel(DeviceProperty[] properties = null, DeviceModelCommand[] commands = null)
        {
            var deviceModel = new LoRaDeviceModel
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = new Uri($"http://fake.local/{this.mockModelId}"),
                SupportLoRaFeatures = true
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, LorawanApiBaseUrl)
                    .RespondJson(deviceModel);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/avatar")
                    .RespondText($"http://fake.local/{this.mockModelId}");

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/commands")
                .RespondJson(commands ?? Array.Empty<DeviceModelCommand>());

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/properties")
                    .RespondJson(properties ?? Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient.Fallback
                .With(_ =>
                {
                    Debugger.Break();
                    return true;
                });

            return deviceModel;
        }

        [Test]

        public void ReturnButtonMustNavigateToPreviousPage()
        {

            // Arrange

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{ApiBaseUrl}/properties")
                .RespondJson(new List<DeviceProperty>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new DeviceModel());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{ApiBaseUrl}/avatar")
                .RespondText(string.Empty);

            //_ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            var cut = RenderComponent<DeviceModelDetailPage>(ComponentParameter.CreateParameter("ModelId", this.mockModelId ));
            var returnButton = cut.WaitForElement("#returnButton", TimeSpan.FromSeconds(2));

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));
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
