// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
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
            _ = this.testContext.JSInterop.SetupVoid("mudElementRef.restoreFocus", _ => true);
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


            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            var link = cut.WaitForElements("a.detail-link");

            Assert.IsNotNull(link);
            foreach (var item in link)
            {
                Assert.AreEqual($"device-models/{deviceId}?isLora=true", item.GetAttribute("href"));
            }

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
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


            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            var link = cut.WaitForElements("a.detail-link");

            Assert.IsNotNull(link);
            foreach (var item in link)
            {
                Assert.AreEqual($"device-models/{deviceId}", item.GetAttribute("href"));
            }

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
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
            var grid = cut.WaitForElement("div.mud-grid");

            Assert.IsNotNull(cut.Markup);
            Assert.IsNotNull(grid.InnerHtml);
            cut.WaitForAssertion(() => Assert.AreEqual("Device Models", cut.Find(".mud-typography-h6").TextContent));
            cut.WaitForAssertion(() => Assert.AreEqual(3, cut.FindAll("tr").Count));
            cut.WaitForAssertion(() => Assert.IsNotNull(cut.Find(".mud-table-container")));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
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

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForElement("#addDeviceModelButton").Click();
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("device-models/new", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
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
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
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
            cut.WaitForAssertion(() => Assert.IsNotEmpty(cut.Markup));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
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

            // Act
            var cut = RenderComponent<DeviceModelListPage>();
            cut.WaitForAssertion(() => cut.Find("#tableRefreshButton"));

            for (var i = 0; i < 3; i++)
            {
                cut.WaitForElement("#tableRefreshButton").Click();
                await Task.Delay(0);
            }

            var matchCount = this.mockHttpClient.GetMatchCount(apiCall);
            Assert.AreEqual(4, matchCount);

            // Assert
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            var deviceId = Guid.NewGuid().ToString();
            _ = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var mockDialogReference = this.mockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReloadDeviceModelIfConfirmed()
        {
            var deviceId = Guid.NewGuid().ToString();
            var apiCall = this.mockHttpClient
                .When(HttpMethod.Get, this.apiBaseUrl)
                .RespondJson(new DeviceModel[] { new DeviceModel { ModelId = deviceId, SupportLoRaFeatures = true } });

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var mockDialogReference = this.mockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceModelListPage>();

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            var matchCount = this.mockHttpClient.GetMatchCount(apiCall);
            Assert.AreEqual(2, matchCount);

            // Assert            
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
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
