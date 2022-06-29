// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DeviceConfigurations
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Pages.DeviceConfigurations;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateDeviceConfigurationsPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderCorrectly()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(Array.Empty<DeviceModel>());

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Instance.IsLoading.Should().BeFalse());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void DeviceConfigurationDetailShouldCreateConfiguration()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();
            var configuration = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                Priority = 1,
                Tags = new Dictionary<string, string>(),
                Properties = new Dictionary<string, string>()
            };

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(Array.Empty<DeviceModel>());

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/device-configurations")
                .RespondText(string.Empty)
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceConfig>>(m.Content);
                    var jsonContent = m.Content as ObjectContent<DeviceConfig>;

                    Assert.IsAssignableFrom<DeviceConfig>(jsonContent.Value);
                    Assert.AreEqual(configuration, jsonContent.Value);

                    return true;
                });

            var cut = RenderComponent<CreateDeviceConfigurationsPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            cut.Instance.Configuration = configuration;


            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Instance.IsLoading.Should().BeFalse());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void DeviceConfigurationDetailShouldProcessProblemDetailsExceptionWhenIssueOccursOnCreatingConfiguration()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();
            var configuration = new DeviceConfig
            {
                ConfigurationId = Guid.NewGuid().ToString(),
                ModelId = Guid.NewGuid().ToString(),
                Priority = 1,
                Tags = new Dictionary<string, string>(),
                Properties = new Dictionary<string, string>()
            };

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(Array.Empty<DeviceModel>());

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(Array.Empty<DeviceTag>());

            _ = MockHttpClient.When(HttpMethod.Put, $"/api/device-configurations")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()))
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceConfig>>(m.Content);
                    var jsonContent = m.Content as ObjectContent<DeviceConfig>;

                    Assert.IsAssignableFrom<DeviceConfig>(jsonContent.Value);
                    Assert.AreEqual(configuration, jsonContent.Value);

                    return true;
                });

            var cut = RenderComponent<CreateDeviceConfigurationsPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            cut.Instance.Configuration = configuration;


            // Act
            cut.Find("#saveButton").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Instance.IsLoading.Should().BeFalse());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingModels()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Instance.IsLoading.Should().BeFalse());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingTags()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(Array.Empty<DeviceModel>());

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Instance.IsLoading.Should().BeFalse());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void WhenClickToDeleteTagShouldRemoveTheSelectedTag()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            var modelId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(Array.Empty<DeviceModel>());

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient
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
            _ = cut.Instance.Configuration.Tags.Count.Should().Be(1);
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void WhenClickToAddTagShouldAddTheSelectedTag()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();
            var modelId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(new DeviceModel[]
                {
                    new ()
                    {
                        ModelId = modelId,
                        Name = Guid.NewGuid().ToString()
                    }
                });

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient
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
            _ = cut.Instance.Configuration.Tags.Count.Should().Be(1);
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }
    }
}
