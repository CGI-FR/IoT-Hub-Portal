// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using AzureIoTHub.Portal.Models;
    using AzureIoTHub.Portal.Models.v10;
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
    using AzureIoTHub.Portal.Client.Shared;

    [TestFixture]
    public class CreateDeviceModelPageTests : IDisposable
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
#pragma warning restore CA2213 // Disposable fields should be disposed

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        private static string ApiBaseUrl => "/api/models";
        // private static string LorawanApiUrl => "/api/lorawan/models";

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
            var modelName = Guid.NewGuid().ToString();
            var description = Guid.NewGuid().ToString();

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceModel>>(m.Content);
                    var jsonContent = m.Content as ObjectContent<DeviceModel>;

                    Assert.IsAssignableFrom<DeviceModel>(jsonContent.Value);
                    var deviceModel = jsonContent.Value as DeviceModel;

                    Assert.IsNotNull(deviceModel.ModelId);
                    Assert.AreEqual(deviceModel.Name, modelName);
                    Assert.AreEqual(deviceModel.Description, description);
                    Assert.AreEqual(deviceModel.SupportLoRaFeatures, false);

                    return true;
                })
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ ApiBaseUrl }/*/properties")
                .RespondText(string.Empty);

            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            cut.Find($"#{nameof(DeviceModel.Name)}").Change(modelName);
            cut.Find($"#{nameof(DeviceModel.Description)}").Change(description);

            saveButton.Click();
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ClickOnAddPropertyShouldAddNewProperty()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ ApiBaseUrl}")
                .RespondText(string.Empty);

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ ApiBaseUrl }/*/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var jsonContent = m.Content as ObjectContent<List<DeviceProperty>>;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(1, properties?.Count());

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

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            addPropertyButton.Click();

            cut.WaitForElement($"#property- #{nameof(DeviceProperty.Name)}").Change(propertyName);

            var propertyCssSelector = $"#property-{propertyName}";

            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.DisplayName)}").Change(displayName);
            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.PropertyType)}").Change(nameof(DevicePropertyType.Boolean));
            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.IsWritable)}").Change(true);

            saveButton.Click();
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ClickOnRemovePropertyShouldRemoveTheProperty()
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ ApiBaseUrl }")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ ApiBaseUrl }/*/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var jsonContent = m.Content as ObjectContent<List<DeviceProperty>>;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(0, properties?.Count());

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

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            removePropertyButton.Click();

            saveButton.Click();
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void WhenLoraFeatureIsDisabledModelDetailsShouldNotDisplayLoRaWANSwitch()
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            _ = cut.WaitForElement("#form");

            // Assert
            Assert.AreEqual(0, cut.FindAll("#SupportLoRaFeatures").Count);
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void WhenLoraFeatureIsEnabledModelDetailsShouldDisplayLoRaWANSwitch()
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            _ = cut.WaitForElement("#form");
            _ = cut.WaitForElement("#SupportLoRaFeatures");

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void WhenLoraFeatureIsEnabledModelDetailsShouldDisplayLoRaWANTab()
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<CreateDeviceModelPage>();
            _ = cut.WaitForElement("#form");

            // Act
            cut.WaitForElement("#SupportLoRaFeatures")
                .Change(true);

            cut.WaitForState(() => cut.FindAll(".mud-tabs .mud-tab").Count == 2);

            // Assert
            var tabs = cut.FindAll(".mud-tabs .mud-tab");
            Assert.AreEqual(2, tabs.Count);
            Assert.AreEqual("General", tabs[0].TextContent);
            Assert.AreEqual("LoRaWAN", tabs[1].TextContent);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        //[Test]
        //public void WhenLoraDeviceModelDetailsShouldCallLoRaAPIs()
        //{
        //    // Arrange
        //    _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

        //    var cut = RenderComponent<CreateDeviceModelPage>();
        //    _ = cut.WaitForElement("#form");

        //    cut.Find($"#{nameof(DeviceModel.Name)}").Change(Guid.NewGuid().ToString());
        //    cut.Find($"#{nameof(DeviceModel.Description)}").Change(Guid.NewGuid().ToString());

        //    cut.WaitForElement("#SupportLoRaFeatures")
        //        .Change(true);

        //    var tabs = cut.FindAll(".mud-tabs .mud-tab");
        //    var loraTab = tabs[1];
        //    loraTab.Click();
        //    var actualCreateLoraDeviceModel = cut.FindComponent<CreateLoraDeviceModel>();

        //    // cut.Render();
        //    // cut.FindComponent<CreateLoraDeviceModel>()
        //    //    .Find($"#{nameof(LoRaDeviceModel.AppEUI)}")
        //    //    .Change(Guid.NewGuid().ToString());

        //    _ = this.mockHttpClient.When(HttpMethod.Post, $"{LorawanApiUrl}")
        //        .RespondText(string.Empty);

        //    _ = this.mockHttpClient.When(HttpMethod.Post, $"{ LorawanApiUrl }/*/commands")
        //        .RespondText(string.Empty);

        //    _ = this.mockHttpClient.When(HttpMethod.Post, $"{ LorawanApiUrl }/*/avatar")
        //        .RespondText(string.Empty);

        //    var saveButton = cut.WaitForElement("#SaveButton");

        //    // Act
        //    saveButton.Click();
        //    cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

        //    // Assert
        //    this.mockHttpClient.VerifyNoOutstandingExpectation();
        //}

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
