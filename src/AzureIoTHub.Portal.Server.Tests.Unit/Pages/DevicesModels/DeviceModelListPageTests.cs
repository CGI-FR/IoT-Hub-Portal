// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DevicesModels
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using Models.v10;
    using Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeviceModelListPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;

        private readonly string apiBaseUrl = "/api/models";

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
        }

        [Test]
        public void WhenLoraFeatureDisableClickToItemShouldRedirectToDeviceDetailsPage()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = modelId, SupportLoRaFeatures = false } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/device-models/{modelId}"));
        }

        [Test]
        public void WhenLoraFeatureEnableClickToItemShouldRedirectToLoRaDeviceDetailsPage()
        {
            // Arrange
            var modelId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = modelId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/device-models/{modelId}?isLora=true"));
        }

        [Test]
        public void DeviceModelListPageRendersCorrectly()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] {
                    new DeviceModel { ModelId = Guid.NewGuid().ToString() },
                    new DeviceModel{  ModelId = Guid.NewGuid().ToString() }
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            var grid = cut.WaitForElement("div.mud-grid");

            Assert.IsNotNull(cut.Markup);
            Assert.IsNotNull(grid.InnerHtml);
            cut.WaitForAssertion(() => Assert.AreEqual("Device Models", cut.Find(".mud-typography-h6").TextContent));
            cut.WaitForAssertion(() => Assert.AreEqual(3, cut.FindAll("tr").Count));
            cut.WaitForAssertion(() => Assert.IsNotNull(cut.Find(".mud-table-container")));

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenAddNewDeviceModelClickShouldNavigateToNewDeviceModelPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForElement("#addDeviceModelButton").Click();
            cut.WaitForState(() => Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("device-models/new", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LoadDeviceModelsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void LoadDeviceModelsShouldDisplayAccessTokenNotAvailableExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = MockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}")
                .Throw(new AccessTokenNotAvailableException(Services.GetRequiredService<FakeNavigationManager>(), new AccessTokenResult(AccessTokenResultStatus.Success, new AccessToken(), "redirectUrl"), null));

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => Assert.IsNotEmpty(cut.Markup));
            cut.WaitForState(() => Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/redirectUrl", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public async Task WhenRefreshClickShouldReloadFromApi()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var apiCall = MockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Find("#tableRefreshButton"));

            for (var i = 0; i < 3; i++)
            {
                cut.WaitForElement("#tableRefreshButton").Click();
                await Task.Delay(0);
            }

            var matchCount = MockHttpClient.GetMatchCount(apiCall);
            Assert.AreEqual(4, matchCount);

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            var deviceId = Guid.NewGuid().ToString();
            _ = MockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReloadDeviceModelIfConfirmed()
        {
            var deviceId = Guid.NewGuid().ToString();
            var apiCall = MockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            var matchCount = MockHttpClient.GetMatchCount(apiCall);
            Assert.AreEqual(2, matchCount);

            // Assert            
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
