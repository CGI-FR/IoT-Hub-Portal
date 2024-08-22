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
    using UnitTests.Helpers;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Shared;
    using Portal.Shared.Models.v1._0;
    using Portal.Shared.Models.v1._0.Filters;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateDeviceConfigurationsPageTests : BlazorUnitTest
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
            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto> { Items = new List<DeviceModelDto>() });

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailShouldCreateConfiguration()
        {
            // Arrange
            var configuration = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                Priority = 1,
                Tags = new Dictionary<string, string>(),
                Properties = new Dictionary<string, string>()
            };

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto> { Items = new List<DeviceModelDto>() });

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.CreateDeviceConfiguration(configuration))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<CreateDeviceConfigurationsPage>();
            cut.Instance.Configuration = configuration;


            // Act
            cut.WaitForElement("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailShouldProcessProblemDetailsExceptionWhenIssueOccursOnCreatingConfiguration()
        {
            // Arrange
            var configuration = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                Priority = 1,
                Tags = new Dictionary<string, string>(),
                Properties = new Dictionary<string, string>()
            };

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto> { Items = new List<DeviceModelDto>() });

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceConfigurationsClientService.Setup(service =>
                    service.CreateDeviceConfiguration(configuration))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<CreateDeviceConfigurationsPage>();
            cut.Instance.Configuration = configuration;


            // Act
            cut.WaitForElement("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingModels()
        {
            // Arrange
            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingTags()
        {
            // Arrange
            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto> { Items = new List<DeviceModelDto>() });

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickToDeleteTagShouldRemoveTheSelectedTag()
        {
            // Arrange
            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto> { Items = new List<DeviceModelDto>() });

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            cut.Instance.SelectedTag = "tag0";
            cut.Render();
            cut.WaitForElement("#addTagButton").Click();

            cut.Instance.SelectedTag = "tag1";
            cut.Render();
            cut.WaitForElement("#addTagButton").Click();

            // Act
            cut.WaitForElement("#tag-tag1 #deleteTagButton").Click();

            // Assert
            _ = cut.Instance.Configuration.Tags.Count.Should().Be(1);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenClickToAddTagShouldAddTheSelectedTag()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModels(It.IsAny<DeviceModelFilter>()))
                .ReturnsAsync(new PaginationResult<DeviceModelDto> { Items = new[] { new DeviceModelDto { ModelId = modelId, Name = Guid.NewGuid().ToString() } } });

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockDeviceTagSettingsClientService.Setup(service =>
                    service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new () { Name = "tag0" },
                    new () { Name = "tag1" }
                });

            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Act
            cut.Instance.SelectedTag = "tag1";
            cut.Render();
            cut.WaitForElement("#addTagButton").Click();

            // Assert
            _ = cut.WaitForElement("#tag-tag1");
            _ = cut.Instance.Configuration.Tags.Count.Should().Be(1);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
