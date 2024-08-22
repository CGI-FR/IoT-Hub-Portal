// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.DeviceConfigurations
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.DeviceConfigurations;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Shared.Models;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class DeviceConfigurationDetailPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<IDeviceConfigurationsClientService> mockDeviceConfigurationsClientService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockDeviceConfigurationsClientService = MockRepository.Create<IDeviceConfigurationsClientService>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockDeviceConfigurationsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderCorrectly()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId
                });

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Assert
            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConfiguration()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Assert
            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b></b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {DateTime.MinValue}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">0 devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">0 devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteDeviceConfigurationShouldShowDeleteDialog()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId
                });

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceConfiguration>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Act
            cut.WaitForElement("#delete-device-configuration").Click();

            // Assert
            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderCardCorrectly()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId
                });

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(model);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Assert
            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderTags()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Assert
            _ = cut.WaitForElement("#tag-tag0");
            _ = cut.WaitForElement("#tag-tag1");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickToDeleteTagShouldRemoveTheSelectedTag()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Act
            var deleteTagButton = cut.WaitForElement("#tag-tag1 #deleteTagButton");
            deleteTagButton.Click();
            cut.WaitForState(() => cut.Instance.Configuration.Tags.Count == 1);

            // Assert
            Assert.AreEqual(1, cut.Instance.Configuration.Tags.Count);

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickToAddTagShouldAddTheSelectedTag()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId,
                    Tags = new Dictionary<string, string>()
                    {
                        { "tag0", "value1" }
                    }
                });

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Act
            cut.Instance.SelectedTag = "tag1";
            cut.Render();
            cut.WaitForElement("#addTagButton").Click();

            // Assert
            _ = cut.WaitForElement("#tag-tag1");
            Assert.AreEqual(2, cut.Instance.Configuration.Tags.Count);

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderProperties()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>
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

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Assert
            _ = cut.WaitForElement("#property-prop1");
            _ = cut.WaitForElement("#property-prop2");
            _ = cut.WaitForElement("#property-prop3");
            _ = cut.WaitForElement("#property-prop4");
            _ = cut.WaitForElement("#property-prop5");
            _ = cut.WaitForElement("#property-prop6");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickToDeletePropertyShouldRemoveTheSelectedProperty()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>
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

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Act
            cut.WaitForElement("#property-prop2 #deletePropertyButton").Click();
            cut.WaitForState(() => cut.Instance.Configuration.Properties.Count == 1);

            // Assert
            Assert.AreEqual(1, cut.Instance.Configuration.Properties.Count);

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickToAddPropertyShouldAddTheSelectedProperty()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>
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

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Act
            cut.Instance.SelectedTag = "prop2";
            cut.Render();
            cut.WaitForElement("#addPropertyButton").Click();

            // Assert
            _ = cut.WaitForElement("#property-prop2");
            Assert.AreEqual(2, cut.Instance.Configuration.Properties.Count);

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickToSaveShouldSendPutToTheEndpoint()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = configurationId,
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>
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

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.UpdateDeviceConfiguration(It.Is<DeviceConfig>(config =>
                        configurationId.Equals(config.ConfigurationId, StringComparison.Ordinal) && modelId.Equals(config.ModelId, StringComparison.Ordinal))))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            // Act
            cut.WaitForElement("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-configurations"));

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnSavingConfiguration()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = configurationId,
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>
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

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.UpdateDeviceConfiguration(It.Is<DeviceConfig>(config =>
                        configurationId.Equals(config.ConfigurationId, StringComparison.Ordinal) &&
                        modelId.Equals(config.ModelId, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));

            cut.WaitForAssertion(() => cut.Find("#saveButton"));

            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {

            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();

            var model = new DeviceModelDto
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

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfiguration(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceConfig
                {
                    Priority = 100,
                    ConfigurationId = "test",
                    ModelId = modelId
                });

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.GetDeviceConfigurationMetrics(It.Is<string>(s =>
                        configurationId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(metrics);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(It.Is<string>(s => modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = modelId,
                    Name = model.Name
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(It.Is<string>(s =>
                        modelId.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceConfigurationDetailPage>(ComponentParameter.CreateParameter("ConfigId", configurationId));
            cut.WaitForAssertion(() => cut.Find("#returnButton"));

            // Act
            cut.Find("#returnButton").Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-configurations"));

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(1) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Model: <b>{model.Name}</b></p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(3) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">Created at {metrics.CreationDate}</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(4) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsTargeted} devices targeted</p>");

            cut.Find("div.mud-card-content > .mud-grid > .mud-grid-item:nth-child(5) > p")
                .MarkupMatches($"<p class=\"mud-typography mud-typography-body1\">{metrics.MetricsApplied} devices applied</p>");

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
