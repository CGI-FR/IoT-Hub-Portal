// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Devices
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Tests.Unit.Extensions;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DevicesListPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        private readonly string apiBaseUrl = "/api/devices";
        private readonly string apiTagsBaseUrl = "/api/settings/device-tags";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            this.mockHttpClient = this.testContext.Services.AddMockHttpClient();

            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);
            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudElementRef.restoreFocus", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void DeviceListPageRendersCorrectly()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiTagsBaseUrl)
                .RespondJson(Array.Empty<object>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState=")
                .RespondJson(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Find(".mud-expansion-panels .mud-expand-panel .mud-expand-panel-header .mud-expand-panel-text").TextContent.Should().Be("Search panel"));
            cut.WaitForAssertion(() => cut.Find(".mud-expansion-panels .mud-expand-panel").ClassList.Should().NotContain("Search panel should be collapsed"));

            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public async Task WhenResetFilterButtonClickShouldClearFilters()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState=")
                .RespondJson(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiTagsBaseUrl)
                .RespondJson(Array.Empty<object>());


            // Act
            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForElement("#searchID").NodeValue = Guid.NewGuid().ToString();
            cut.WaitForElement("#searchStatusEnabled").Click();
            cut.WaitForElement("#searchStateDisconnected").Click();

            cut.WaitForElement("#resetSearch").Click();
            await Task.Delay(100);

            // Assert
            cut.WaitForAssertion(() => Assert.IsNull(cut.Find("#searchID").NodeValue));
            cut.WaitForAssertion(() => Assert.AreEqual("false", cut.Find("#searchStatusEnabled").Attributes["aria-checked"].Value));
            cut.WaitForAssertion(() => Assert.AreEqual("false", cut.Find("#searchStateDisconnected").Attributes["aria-checked"].Value));
            cut.WaitForAssertion(() => Assert.AreEqual("true", cut.Find("#searchStatusAll").Attributes["aria-checked"].Value));
            cut.WaitForAssertion(() => Assert.AreEqual("true", cut.Find("#searchStateAll").Attributes["aria-checked"].Value));

            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void WhenAddNewDeviceClickShouldNavigateToNewDevicePage()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState=")
                .RespondJson(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiTagsBaseUrl)
                .RespondJson(Array.Empty<object>());

            var mockNavigationManager = this.testContext.Services.GetRequiredService<FakeNavigationManager>();


            // Act
            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForElement("#addDeviceButton").Click();
            cut.WaitForAssertion(() => string.Equals("http://localhost/devices/new", mockNavigationManager.Uri, StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public async Task WhenRefreshClickShouldReloadFromApi()
        {
            // Arrange
            var apiCall = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState=")
                .RespondJson(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiTagsBaseUrl)
                .RespondJson(Array.Empty<object>());


            // Act
            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Find("#tableRefreshButton"));

            for (var i = 0; i < 3; i++)
            {
                cut.Find("#tableRefreshButton").Click();
                await Task.Delay(100);
            }

            // Assert
            var matchCount = this.mockHttpClient.GetMatchCount(apiCall);
            Assert.AreEqual(4, matchCount);
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureDisableClickToItemShouldRedirectToDeviceDetailsPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState=")
                .RespondJson(new PaginationResult<DeviceListItem>
                {
                    Items = new DeviceListItem[] { new DeviceListItem { DeviceID = deviceId } }
                });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiTagsBaseUrl)
                .RespondJson(Array.Empty<object>());

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => this.testContext.Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/devices/{deviceId}"));
        }

        [Test]
        public void WhenLoraFeatureEnableClickToItemShouldRedirectToLoRaDeviceDetailsPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState=")
                .RespondJson(new PaginationResult<DeviceListItem>
                {
                    Items = new DeviceListItem[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true } }
                });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiTagsBaseUrl)
                .RespondJson(Array.Empty<object>());

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => this.testContext.Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/devices/{deviceId}?isLora=true"));
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceTags()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState=")
                .RespondJson(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiTagsBaseUrl)
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void LoadItemsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDevices()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState=")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiTagsBaseUrl)
                .RespondJson(Array.Empty<object>());

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

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
