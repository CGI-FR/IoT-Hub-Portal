// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using System;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Services;
    using Client.Shared;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Moq;
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class AppbarTests : TestContextWrapper
    {
        private TestAuthorizationContext authContext;
        private FakeNavigationManager fakeNavigationManager;
        private MockRepository mockRepository;

        [SetUp]
        public void Setup()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();
            this.authContext = TestContext.AddTestAuthorization();
            _ = TestContext.AddBlazoredLocalStorage();
            _ = TestContext.Services.AddScoped<ILayoutService, LayoutService>();
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.fakeNavigationManager = TestContext.Services.GetRequiredService<FakeNavigationManager>();

            _ = TestContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        [Test]
        public void AppBarShouldRendersCorrectly()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());

            // Act
            var cut = RenderComponent<Appbar>();

            // Assert
            _ = cut.Markup.Should().NotBeNullOrEmpty();
            _ = cut.FindAll("button.mud-button-root").Count.Should().Be(3);
            _ = cut.FindAll("div.mud-avatar").Count.Should().Be(1);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnUserMenuShouldOpenUserMenuOverlay()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("div.mud-menu-activator"));

            // Act
            cut.Find("div.mud-menu-activator").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Find("div.mud-overlay"));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnOpenUserMenuShouldCloseUserMenuOverlay()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("div.mud-menu-activator"));
            cut.Find("div.mud-menu-activator").Click();
            cut.WaitForAssertion(() => cut.Find("div.mud-overlay"));

            // Act
            cut.Find("div.mud-menu-activator").Click();

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("div.mud-overlay").Count.Should().Be(0));
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnDarkModeShouldSetDarkMode()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("#dark_light_switch button"));

            // Act
            cut.Find("#dark_light_switch button").Click();

            // Assert
            _ = TestContext?.Services.GetRequiredService<ILayoutService>().IsDarkMode.Should().BeTrue();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void ClickOnLightModeShouldSetLightMode()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());
            var cut = RenderComponent<Appbar>();
            cut.WaitForAssertion(() => cut.Find("#dark_light_switch button"));

            // Act
            cut.Find("#dark_light_switch button").Click();
            cut.Find("#dark_light_switch button").Click();

            // Assert
            _ = TestContext?.Services.GetRequiredService<ILayoutService>().IsDarkMode.Should().BeFalse();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void LoginMustNotBeShownWhenUserIsAuthorized()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());

            // Act
            var cut = RenderComponent<Appbar>();

            // Assert
            _ = cut.Markup.Should().NotBeNullOrEmpty();
            _ = cut.FindAll("#login").Count.Should().Be(0);
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void LoginMustBeShownWhenUserIsNotAuthorized()
        {
            // Arrange
            _ = this.authContext.SetNotAuthorized();

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

            // Act
            var cut = RenderComponent<Appbar>();
            cut.Find("#login").Click();

            // Assert
            _ = cut.Markup.Should().NotBeNullOrEmpty();
            cut.WaitForState(() => this.fakeNavigationManager.Uri.EndsWith("authentication/login", StringComparison.OrdinalIgnoreCase));
        }
    }
}
