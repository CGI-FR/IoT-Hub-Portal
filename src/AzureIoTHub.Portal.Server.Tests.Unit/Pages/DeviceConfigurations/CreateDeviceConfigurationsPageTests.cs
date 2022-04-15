// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using Bunit;
    using Client.Pages.DeviceConfigurations;
    using Helpers;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using Portal.Shared.Models.v10;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateDeviceConfigurationsPageTests : IDisposable
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
#pragma warning restore CA2213 // Disposable fields should be disposed

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = this.testContext.Services.AddMockHttpClient();
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

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
        public void DeviceConfigurationDetailPageShouldRenderCorrectly()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(Array.Empty<DeviceModel>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForState(() => !cut.Instance.IsLoading);
        }

        [Test]
        public void WhenClickToDeleteTagShouldRemoveTheSelectedTag()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(Array.Empty<DeviceModel>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(new DeviceTag[]
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            var cut = RenderComponent<CreateDeviceConfigurationsPage>();
            cut.WaitForState(() => !cut.Instance.IsLoading);

            cut.Instance.SelectedTag = "tag0";
            cut.Render();
            cut.WaitForElement("#addTagButton").Click();

            cut.Instance.SelectedTag = "tag1";
            cut.Render();
            cut.WaitForElement("#addTagButton").Click();

            // Act
            var deleteTagButton = cut.WaitForElement("#tag-tag1 #deleteTagButton");
            deleteTagButton.Click();

            // Assert
            Assert.AreEqual(1, cut.Instance.Configuration.Tags.Count);
        }

        [Test]
        public void WhenClickToAddTagShouldAddTheSelectedTag()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(new DeviceModel[]
                {
                    new ()
                    {
                        ModelId = modelId,
                        Name = Guid.NewGuid().ToString()
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(new DeviceTag[]
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            var cut = RenderComponent<CreateDeviceConfigurationsPage>();
            cut.WaitForState(() => !cut.Instance.IsLoading);

            // Act
            cut.Instance.SelectedTag = "tag1";
            cut.Render();
            cut.WaitForElement("#addTagButton").Click();

            // Assert
            _ = cut.WaitForElement("#tag-tag1");
            Assert.AreEqual(1, cut.Instance.Configuration.Tags.Count);
        }

        //[Test]
        //public void WhenClickToDeletePropertyShouldRemoveTheSelectedProperty()
        //{
        //    // Arrange
        //    using var deviceResponseMock = new HttpResponseMessage();

        //    var modelId = Guid.NewGuid().ToString();

        //    var model = new DeviceModel
        //    {
        //        ModelId = modelId,
        //        Name = Guid.NewGuid().ToString()
        //    };

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, "/api/models")
        //        .RespondJson(new []
        //        {
        //            model
        //        });

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
        //        .RespondJson(new DeviceProperty[]
        //        {
        //            new() {
        //                Name = "prop1",
        //                PropertyType = DevicePropertyType.Boolean
        //            },
        //            new() {
        //                Name = "prop2",
        //                PropertyType = DevicePropertyType.Double
        //            },
        //            new() {
        //                Name = "prop3",
        //                PropertyType = DevicePropertyType.Float
        //            },
        //            new() {
        //                Name = "prop4",
        //                PropertyType = DevicePropertyType.Integer
        //            },
        //            new() {
        //                Name = "prop5",
        //                PropertyType = DevicePropertyType.Long
        //            },
        //            new() {
        //                Name = "prop6",
        //                PropertyType = DevicePropertyType.String
        //            }
        //        });

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, "/api/settings/device-tags")
        //        .RespondJson(Array.Empty<DeviceTag>());

        //    var cut = RenderComponent<CreateDeviceConfigurationsPage>();
        //    cut.WaitForState(() => !cut.Instance.IsLoading);
        //    cut.WaitForElement("#selectDeviceModel").Change(model);

        //    cut.Instance.SelectedTag = "prop1";
        //    cut.Render();
        //    cut.WaitForElement("#addPropertyButton").Click();

        //    cut.Instance.SelectedTag = "prop2";
        //    cut.Render();
        //    cut.WaitForElement("#addPropertyButton").Click();

        //    // Act
        //    cut.WaitForElement("#property-prop2 #deletePropertyButton").Click();

        //    // Assert
        //    Assert.AreEqual(1, cut.Instance.Configuration.Properties.Count);
        //}

        //[Test]
        //public async Task WhenClickToAddPropertyShouldAddTheSelectedProperty()
        //{
        //    // Arrange
        //    using var deviceResponseMock = new HttpResponseMessage();

        //    var modelId = Guid.NewGuid().ToString();

        //    var model = new DeviceModel
        //    {
        //        ModelId = modelId,
        //        Name = Guid.NewGuid().ToString()
        //    };

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, "/api/models")
        //        .RespondJson(new[]
        //        {
        //            model
        //        });

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
        //        .RespondJson(new DeviceProperty[]
        //        {
        //            new() {
        //                Name = "prop1",
        //                PropertyType = DevicePropertyType.Boolean,
        //                IsWritable = true
        //            },
        //            new() {
        //                Name = "prop2",
        //                PropertyType = DevicePropertyType.Double,
        //                IsWritable = true
        //            },
        //            new() {
        //                Name = "prop3",
        //                PropertyType = DevicePropertyType.Float,
        //                IsWritable = true
        //            },
        //            new() {
        //                Name = "prop4",
        //                PropertyType = DevicePropertyType.Integer,
        //                IsWritable = true
        //            },
        //            new() {
        //                Name = "prop5",
        //                PropertyType = DevicePropertyType.Long,
        //                IsWritable = true
        //            },
        //            new() {
        //                Name = "prop6",
        //                PropertyType = DevicePropertyType.String,
        //                IsWritable = true
        //            }
        //        });

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, "/api/settings/device-tags")
        //        .RespondJson(Array.Empty<DeviceTag>());

        //    var cut = RenderComponent<CreateDeviceConfigurationsPage>();
        //    cut.WaitForState(() => !cut.Instance.IsLoading);
        //    await cut.Instance.SelectModel(model);
        //    cut.Render();

        //    // Act
        //    cut.Instance.SelectedTag = "prop1";
        //    cut.Find("#addPropertyButton").Click();

        //    // Assert
        //    _ = cut.WaitForElement("#property-prop1");
        //    Assert.AreEqual(1, cut.Instance.Configuration.Properties.Count);
        //}

        //[Test]
        //public void WhenClickToSaveShouldSendPutToTheEndpoint()
        //{
        //    // Arrange
        //    using var deviceResponseMock = new HttpResponseMessage();

        //    var configurationId = Guid.NewGuid().ToString();
        //    var modelId = Guid.NewGuid().ToString();

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
        //        .RespondJson(new DeviceConfig
        //        {
        //            Priority = 100,
        //            ConfigurationId = "test",
        //            ModelId = modelId,
        //            Tags = new Dictionary<string, string>()
        //            {
        //                { "tag0", "value1" }
        //            },
        //            Properties = new Dictionary<string, string>()
        //            {
        //                { "prop1", "val1" },
        //                { "prop2", "val2" }
        //            }
        //        });

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
        //        .RespondJson(new ConfigurationMetrics());

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
        //        .RespondJson(new DeviceProperty[]
        //        {
        //            new() {
        //                Name = "prop1",
        //                PropertyType = DevicePropertyType.Boolean
        //            },
        //            new() {
        //                Name = "prop2",
        //                PropertyType = DevicePropertyType.Double
        //            },
        //            new() {
        //                Name = "prop3",
        //                PropertyType = DevicePropertyType.Float
        //            },
        //            new() {
        //                Name = "prop4",
        //                PropertyType = DevicePropertyType.Integer
        //            },
        //            new() {
        //                Name = "prop5",
        //                PropertyType = DevicePropertyType.Long
        //            },
        //            new() {
        //                Name = "prop6",
        //                PropertyType = DevicePropertyType.String
        //            }
        //        });

        //    _ = this.mockHttpClient
        //        .When(HttpMethod.Get, "/api/settings/device-tags")
        //        .RespondJson(new DeviceTag[]
        //        {
        //            new () { Name = "tag0" },
        //            new () { Name = "tag1" }
        //        });

        //    var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
        //    cut.WaitForState(() => !cut.Instance.IsLoading);

        //    _ = this.mockHttpClient.When(HttpMethod.Put, $"/api/device-configurations/{configurationId}")
        //        .RespondText(string.Empty)
        //        .With(m =>
        //        {
        //            Assert.IsAssignableFrom<JsonContent>(m.Content);
        //            var jsonContent = m.Content as JsonContent;

        //            Assert.IsAssignableFrom<DeviceConfig>(jsonContent.Value);
        //            Assert.AreEqual(cut.Instance.Configuration, jsonContent.Value);

        //            return true;
        //        });

        //    // Act
        //    cut.WaitForElement("#saveButton").Click();

        //    // Assert
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
