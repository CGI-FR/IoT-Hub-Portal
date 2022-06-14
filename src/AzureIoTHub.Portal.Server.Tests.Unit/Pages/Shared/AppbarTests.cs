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
        [SetUp]
        public void Setup()
        {
            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();
            var authContext = TestContext.AddTestAuthorization();
            _ = authContext.SetAuthorized(Guid.NewGuid().ToString());
            _ = TestContext.Services.AddSingleton<ILayoutService>(new LayoutService());
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = TestContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        [Test]
        public void AppBarShouldRendersCorrectly()
        {
            // Act
            var cut = RenderComponent<Appbar>();

            // Assert
            _ = cut.Markup.Should().NotBeNullOrEmpty();
            _ = cut.FindAll("button.mud-button-root").Count.Should().Be(2);
            _ = cut.FindAll("div.mud-avatar").Count.Should().Be(1);
        }
    }
}
