// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using System;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Pages.Ideas;
    using Client.Services;
    using Client.Shared;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class AppbarTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;

        private TestAuthorizationContext authContext;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);

            this.authContext = TestContext?.AddTestAuthorization();
            _ = TestContext.AddBlazoredLocalStorage();

            _ = Services.AddScoped<ILayoutService, LayoutService>();
        }

        [Test]
        public void AppBarShouldRendersCorrectly()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });

            // Act
            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("#title"));

            // Assert
            _ = cut.Markup.Should().NotBeNullOrEmpty();
            _ = cut.Find("#title").TextContent.Should().Be("TEST");
            _ = cut.FindAll("button.mud-button-root").Count.Should().Be(3);
            _ = cut.FindAll("div.mud-avatar").Count.Should().Be(1);
        }

        [Test]
        public void ClickOnUserMenuShouldOpenUserMenuOverlay()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });

            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("div.mud-menu-activator"));

            // Act
            cut.Find("div.mud-menu-activator").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Find("div.mud-overlay"));
        }

        [Test]
        public void ClickOnOpenUserMenuShouldCloseUserMenuOverlay()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });

            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("div.mud-menu-activator"));
            cut.Find("div.mud-menu-activator").Click();
            cut.WaitForAssertion(() => cut.Find("div.mud-overlay"));

            // Act
            cut.Find("div.mud-menu-activator").Click();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("div.mud-overlay").Count.Should().Be(0));
        }

        [Test]
        public void ClickOnDarkModeShouldSetDarkMode()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });

            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("#dark_light_switch button"));

            // Act
            cut.Find("#dark_light_switch button").Click();

            // Assert
            _ = TestContext?.Services.GetRequiredService<ILayoutService>().IsDarkMode.Should().BeTrue();
        }

        [Test]
        public void ClickOnLightModeShouldSetLightMode()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });

            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("#dark_light_switch button"));

            // Act
            cut.Find("#dark_light_switch button").Click();
            cut.Find("#dark_light_switch button").Click();

            // Assert
            _ = TestContext?.Services.GetRequiredService<ILayoutService>().IsDarkMode.Should().BeFalse();
        }

        [Test]
        public void LoginMustNotBeShownWhenUserIsAuthorized()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });

            // Act
            var cut = RenderComponent<Appbar>();

            // Assert
            _ = cut.Markup.Should().NotBeNullOrEmpty();
            _ = cut.FindAll("#login").Count.Should().Be(0);
        }

        [Test]
        public void LoginMustBeShownWhenUserIsNotAuthorized()
        {
            // Arrange
            _ = this.authContext.SetNotAuthorized();
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });

            // Act
            var cut = RenderComponent<Appbar>();

            // Assert
            _ = cut.Markup.Should().NotBeNullOrEmpty();
            _ = cut.Find("#login").TextContent.Should().Be("Log in");
        }

        [Test]
        public void CLickOnLoginMustRedirectToAuthenticationLogin()
        {
            // Arrange
            _ = this.authContext.SetNotAuthorized();
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });

            // Act
            var cut = RenderComponent<Appbar>();
            cut.Find("#login").Click();

            // Assert
            _ = cut.Markup.Should().NotBeNullOrEmpty();
            cut.WaitForState(() => Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("authentication/login", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void AppBarShouldRenderIdeaButtonWhenIdeaFeatureIsEnabled()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false,
                IsIdeasFeatureEnabled = true
            });

            // Act
            var cut = RenderComponent<Appbar>();

            // Assert
            cut.WaitForAssertion(() => cut.Find("#ideas"));
        }

        [Test]
        public void OnCLickOnNewIdeaShouldShowIdeaDialog()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false,
                IsIdeasFeatureEnabled = true
            });

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<SubmitIdeaDialog>(string.Empty, It.IsAny<DialogOptions>()))
                .Returns(mockDialogReference);

            var cut = RenderComponent<Appbar>();

            // Act
            cut.WaitForElement("#ideas button").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
