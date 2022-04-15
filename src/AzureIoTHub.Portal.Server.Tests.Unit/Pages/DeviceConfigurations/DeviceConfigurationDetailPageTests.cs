// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
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
    public class DeviceConfigurationDetailPageTests : IDisposable
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

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(new ConfigurationMetrics
                {
                    CreationDate = DateTime.MinValue
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel
                {
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Assert
            cut.WaitForState(() => !cut.Instance.IsLoading);
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderCardCorrectly()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModel
            {
                ModelId = modelId,
                Name = Guid.NewGuid().ToString()
            };

            var metrics = new ConfigurationMetrics
            {
                CreationDate = DateTime.FromOADate(Random.Shared.NextDouble()),
                MetricsApplied = Random.Shared.Next(),
                MetricsFailure = Random.Shared.Next(),
                MetricsSuccess = Random.Shared.Next(),
                MetricsTargeted = Random.Shared.Next()
            };

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(metrics);

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(model);

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForState(() => !cut.Instance.IsLoading);

            // Assert
            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1 mud-inherit-text\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1 mud-inherit-text\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1 mud-inherit-text\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1 mud-inherit-text\">{metrics.MetricsApplied} devices applied</p>");
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderTags()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId,
                    Tags = new Dictionary<string, string>()
                    {
                        { "tag0", "value1" },
                        { "tag1", "value2" }
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(new ConfigurationMetrics());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel
                {
                    Name = Guid.NewGuid().ToString()
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

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForState(() => !cut.Instance.IsLoading);

            // Assert
            _ = cut.WaitForElement("#tag-tag0");
            _ = cut.WaitForElement("#tag-tag1");
        }

        [Test]
        public void WhenClickToDeleteTagShouldRemoveTheSelectedTag()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId,
                    Tags = new Dictionary<string, string>()
                    {
                        { "tag0", "value1" },
                        { "tag1", "value2" }
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(new ConfigurationMetrics());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel
                {
                    Name = Guid.NewGuid().ToString()
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

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForState(() => !cut.Instance.IsLoading);

            // Act
            var deleteTagButton = cut.WaitForElement("#tag-tag1 #deleteTagButton");
            deleteTagButton.Click();
            cut.WaitForState(() => cut.Instance.Configuration.Tags.Count == 1);

            // Assert
            Assert.AreEqual(1, cut.Instance.Configuration.Tags.Count);
        }

        [Test]
        public void WhenClickToAddTagShouldAddTheSelectedTag()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId,
                    Tags = new Dictionary<string, string>()
                    {
                        { "tag0", "value1" }
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(new ConfigurationMetrics());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel
                {
                    Name = Guid.NewGuid().ToString()
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

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForState(() => !cut.Instance.IsLoading);

            // Act
            cut.Instance.SelectedTag = "tag1";
            cut.Render();
            cut.WaitForElement("#addTagButton").Click();

            // Assert
            _ = cut.WaitForElement("#tag-tag1");
            Assert.AreEqual(2, cut.Instance.Configuration.Tags.Count);
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderProperties()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId,
                    Tags = new Dictionary<string, string>(),
                    Properties = new Dictionary<string, string>()
                    {
                        { "prop1", "val1" },
                        { "prop2", "val2" },
                        { "prop3", "val3" },
                        { "prop4", "val4" },
                        { "prop5", "val5" },
                        { "prop6", "val6" }
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(new ConfigurationMetrics());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel
                {
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(new DeviceProperty[]
                {
                    new() {
                        Name = "prop1",
                        PropertyType = DevicePropertyType.Boolean
                    },
                    new() {
                        Name = "prop2",
                        PropertyType = DevicePropertyType.Double
                    },
                    new() {
                        Name = "prop3",
                        PropertyType = DevicePropertyType.Float
                    },
                    new() {
                        Name = "prop4",
                        PropertyType = DevicePropertyType.Integer
                    },
                    new() {
                        Name = "prop5",
                        PropertyType = DevicePropertyType.Long
                    },
                    new() {
                        Name = "prop6",
                        PropertyType = DevicePropertyType.String
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForState(() => !cut.Instance.IsLoading);

            // Assert
            _ = cut.WaitForElement("#property-prop1");
            _ = cut.WaitForElement("#property-prop2");
            _ = cut.WaitForElement("#property-prop3");
            _ = cut.WaitForElement("#property-prop4");
            _ = cut.WaitForElement("#property-prop5");
            _ = cut.WaitForElement("#property-prop6");
        }

        [Test]
        public void WhenClickToDeletePropertyShouldRemoveTheSelectedProperty()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId,
                    Tags = new Dictionary<string, string>(),
                    Properties = new Dictionary<string, string>()
                    {
                        { "prop1", "val1" },
                        { "prop2", "val2" }
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(new ConfigurationMetrics());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel
                {
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(new DeviceProperty[]
                {
                    new() {
                        Name = "prop1",
                        PropertyType = DevicePropertyType.Boolean
                    },
                    new() {
                        Name = "prop2",
                        PropertyType = DevicePropertyType.Double
                    },
                    new() {
                        Name = "prop3",
                        PropertyType = DevicePropertyType.Float
                    },
                    new() {
                        Name = "prop4",
                        PropertyType = DevicePropertyType.Integer
                    },
                    new() {
                        Name = "prop5",
                        PropertyType = DevicePropertyType.Long
                    },
                    new() {
                        Name = "prop6",
                        PropertyType = DevicePropertyType.String
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForState(() => !cut.Instance.IsLoading);

            // Act
            cut.WaitForElement("#property-prop2 #deletePropertyButton").Click();
            cut.WaitForState(() => cut.Instance.Configuration.Properties.Count == 1);

            // Assert
            Assert.AreEqual(1, cut.Instance.Configuration.Properties.Count);
        }

        [Test]
        public void WhenClickToAddPropertyShouldAddTheSelectedProperty()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId,
                    Tags = new Dictionary<string, string>(),
                    Properties = new Dictionary<string, string>()
                    {
                        { "prop1", "val1" },
                        { "prop2", "val2" }
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(new ConfigurationMetrics());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel
                {
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(new DeviceProperty[]
                {
                    new() {
                        Name = "prop1",
                        PropertyType = DevicePropertyType.Boolean
                    },
                    new() {
                        Name = "prop2",
                        PropertyType = DevicePropertyType.Double
                    },
                    new() {
                        Name = "prop3",
                        PropertyType = DevicePropertyType.Float
                    },
                    new() {
                        Name = "prop4",
                        PropertyType = DevicePropertyType.Integer
                    },
                    new() {
                        Name = "prop5",
                        PropertyType = DevicePropertyType.Long
                    },
                    new() {
                        Name = "prop6",
                        PropertyType = DevicePropertyType.String
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForState(() => !cut.Instance.IsLoading);

            // Act
            cut.Instance.SelectedTag = "prop2";
            cut.Render();
            cut.WaitForElement("#addPropertyButton").Click();

            // Assert
            _ = cut.WaitForElement("#property-prop2");
            Assert.AreEqual(2, cut.Instance.Configuration.Properties.Count);
        }

        [Test]
        public void WhenClickToSaveShouldSendPutToTheEndpoint()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}")
                .RespondJson(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId,
                    Tags = new Dictionary<string, string>()
                    {
                        { "tag0", "value1" }
                    },
                    Properties = new Dictionary<string, string>()
                    {
                        { "prop1", "val1" },
                        { "prop2", "val2" }
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/device-configurations/{configurationId}/metrics")
                .RespondJson(new ConfigurationMetrics());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel
                {
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(new DeviceProperty[]
                {
                    new() {
                        Name = "prop1",
                        PropertyType = DevicePropertyType.Boolean
                    },
                    new() {
                        Name = "prop2",
                        PropertyType = DevicePropertyType.Double
                    },
                    new() {
                        Name = "prop3",
                        PropertyType = DevicePropertyType.Float
                    },
                    new() {
                        Name = "prop4",
                        PropertyType = DevicePropertyType.Integer
                    },
                    new() {
                        Name = "prop5",
                        PropertyType = DevicePropertyType.Long
                    },
                    new() {
                        Name = "prop6",
                        PropertyType = DevicePropertyType.String
                    }
                });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(new DeviceTag[]
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForState(() => !cut.Instance.IsLoading);

            _ = this.mockHttpClient.When(HttpMethod.Put, $"/api/device-configurations/{configurationId}")
                .RespondText(string.Empty)
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<DeviceConfig>(jsonContent.Value);
                    Assert.AreEqual(cut.Instance.Configuration, jsonContent.Value);

                    return true;
                });

            // Act
            cut.WaitForElement("#saveButton").Click();

            // Assert
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
