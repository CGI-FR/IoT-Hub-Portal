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
    using Microsoft.AspNetCore.Components.Web.Extensions.Head;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class MainLayoutTests : BlazorUnitTest
    {
        private TestAuthorizationContext authContext;

        public override void Setup()
        {
            base.Setup();

            this.authContext = TestContext?.AddTestAuthorization();

            _ = TestContext.AddBlazoredLocalStorage();
            _ = Services.AddScoped<ILayoutService, LayoutService>();
            _ = Services.AddSingleton(new PortalSettings
            {
                PortalName = "TEST",
                IsLoRaSupported = false
            });
        }

        [Test]
        public void MainLayoutShouldRenderCorrectly()
        {
            // Arrange
            _ = this.authContext.SetAuthorized(Guid.NewGuid().ToString());

            // Act
            var cut = RenderComponent<MainLayout>();
            cut.WaitForAssertion(() => cut.Find("div.mud-main-content"));

            // Assert
            cut.WaitForAssertion(() => cut.FindComponent<MudThemeProvider>().Instance.Should().NotBeNull());
            cut.WaitForAssertion(() => cut.FindComponent<MudDialogProvider>().Instance.Should().NotBeNull());
            cut.WaitForAssertion(() => cut.FindComponent<MudSnackbarProvider>().Instance.Should().NotBeNull());
            cut.WaitForAssertion(() => cut.FindComponent<Title>().Instance.Value.Should().Be("TEST"));
            cut.WaitForAssertion(() => cut.FindComponents<MudAppBar>().Count.Should().Be(2));
            cut.WaitForAssertion(() => cut.FindComponent<Appbar>().Instance.Should().NotBeNull());
            cut.WaitForAssertion(() => cut.FindComponent<MudDrawer>().Instance.Should().NotBeNull());
            cut.WaitForAssertion(() => cut.FindComponent<NavMenu>().Instance.Should().NotBeNull());
            cut.WaitForAssertion(() => cut.FindComponent<PortalFooter>().Instance.Should().NotBeNull());

            _ = cut.Markup.Should().NotBeNullOrEmpty();
            _ = cut.FindAll("div.mud-popover-provider").Count.Should().Be(1);
            _ = cut.FindAll("#mud-snackbar-container").Count.Should().Be(1);
            _ = cut.FindAll("div.mud-layout").Count.Should().Be(1);
            _ = cut.FindAll("div.mud-main-content").Count.Should().Be(1);
            _ = cut.FindAll("#footer").Count.Should().Be(1);
        }
    }
}
