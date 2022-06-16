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
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class AppbarTests : TestContextWrapper
    {
        private TestAuthorizationContext authContext;
        private FakeNavigationManager fakeNavigationManager;

        [SetUp]
        public void Setup()
        {
            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();
            this.authContext = TestContext.AddTestAuthorization();
            _ = TestContext.Services.AddSingleton<ILayoutService>(new LayoutService());
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
            _ = cut.FindAll("button.mud-button-root").Count.Should().Be(2);
            _ = cut.FindAll("div.mud-avatar").Count.Should().Be(1);
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
