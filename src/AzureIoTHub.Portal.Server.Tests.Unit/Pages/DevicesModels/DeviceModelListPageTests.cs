// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions.Extensions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeviceModelListPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        private readonly string apiBaseUrl = "/api/models";

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
        public void WhenLoraFeatureEnableDeviceDetailLinkShouldContainLora()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();
            _ = cut.WaitForElements(".detail-link");

            // Act
            var link = cut.FindAll("a.detail-link");

            // Assert
            Assert.IsNotNull(link);
            foreach (var item in link)
            {
                Assert.AreEqual($"device-models/{deviceId}?isLora=true", item.GetAttribute("href"));
            }
        }

        [Test]
        public void WhenLoraFeatureDisableDeviceDetailLinkShouldNotContainLora()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            var cut = RenderComponent<DeviceModelListPage>();
            _ = cut.WaitForElements(".detail-link");

            // Act
            var link = cut.FindAll("a.detail-link");

            // Assert
            Assert.IsNotNull(link);
            foreach (var item in link)
            {
                Assert.AreEqual($"device-models/{deviceId}", item.GetAttribute("href"));
            }
        }

        [Test]
        public void DeviceModelListPageRendersCorrectly()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] {
                    new DeviceModel { ModelId = Guid.NewGuid().ToString() },
                    new DeviceModel{  ModelId = Guid.NewGuid().ToString() }
                });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            var grid = cut.WaitForElement("div.mud-grid", TimeSpan.FromSeconds(5));
            Thread.Sleep(5000);

            // Assert
            Assert.IsNotNull(cut.Markup);
            Assert.AreEqual("Device Models", cut.Find(".mud-typography-h6").TextContent);
            Assert.IsNotNull(grid.InnerHtml);
            Assert.AreEqual(3, cut.FindAll("tr").Count);
            Assert.IsNotNull(cut.Find(".mud-table-container"));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void WhenAddNewDeviceModelClickShouldNavigateToNewDeviceModelPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();

            // Act
            cut.WaitForElement("#addDeviceModelButton").Click();
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("device-models/new", StringComparison.OrdinalIgnoreCase));

            // Assert
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void LoadDeviceModelsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            var grid = cut.WaitForElement("div.mud-grid", TimeSpan.FromSeconds(5));

            // Assert
            Assert.IsNotEmpty(cut.Markup);
            Assert.IsNotEmpty(grid.InnerHtml);
            Assert.AreEqual(2, cut.FindAll("tr").Count);
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void LoadDeviceModelsShouldDisplayAccessTokenNotAvailableExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{this.apiBaseUrl}")
                .Throw(new AccessTokenNotAvailableException(null, null, null));

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceModelListPage>();

            // Assert
            Assert.IsNotEmpty(cut.Markup);
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task WhenRefreshClickShouldReloadFromApi()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var apiCall = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Find("#tableRefreshButton"), 1.Seconds());

            // Act
            for (var i = 0; i < 3; i++)
            {
                cut.Find("#tableRefreshButton")
                        .Click();
                await Task.Delay(100);
            }

            // Assert
            var matchCount = this.mockHttpClient.GetMatchCount(apiCall);
            Assert.AreEqual(4, matchCount);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            var deviceId = Guid.NewGuid().ToString();
            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");

            var mockDialogReference = this.mockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            deleteButton.Click();

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReloadDeviceModelIfConfirmed()
        {
            var deviceId = Guid.NewGuid().ToString();
            var apiCall = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var cut = RenderComponent<DeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");

            var mockDialogReference = this.mockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));

            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            deleteButton.Click();

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();

            var matchCount = this.mockHttpClient.GetMatchCount(apiCall);
            Assert.AreEqual(2, matchCount);
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
