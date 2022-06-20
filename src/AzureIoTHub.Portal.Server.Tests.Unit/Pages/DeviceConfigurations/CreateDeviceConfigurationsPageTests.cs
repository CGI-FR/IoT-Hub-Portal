// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
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
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateDeviceConfigurationsPageTests : TestContextWrapper, IDisposable
    {
        private MockHttpMessageHandler mockHttpClient;

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        [SetUp]
        public void SetUp()
        {
            TestContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = TestContext.Services.AddMockHttpClient();
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            _ = TestContext.Services.AddSingleton(this.mockDialogService.Object);
            _ = TestContext.Services.AddMudServices();

            TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

            this.mockHttpClient.AutoFlush = true;
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldRenderCorrectly()
        {
            // Arrange
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
            cut.WaitForAssertion(() => cut.Instance.IsLoading.Should().BeFalse());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingModels()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Instance.IsLoading.Should().BeFalse());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void DeviceConfigurationDetailPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingTags()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/models")
                .RespondJson(Array.Empty<DeviceModel>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/settings/device-tags")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDeviceConfigurationsPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Instance.IsLoading.Should().BeFalse());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
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
            _ = cut.Instance.Configuration.Tags.Count.Should().Be(1);
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
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
            _ = cut.Instance.Configuration.Tags.Count.Should().Be(1);
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
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
