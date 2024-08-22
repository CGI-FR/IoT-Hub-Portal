// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Client.Shared;
    using UnitTests.Bases;
    using Blazored.LocalStorage;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Client.Constants;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class NavMenuTests : BlazorUnitTest
    {
        private ILocalStorageService localStorageService;

        public override void Setup()
        {
            base.Setup();

            this.localStorageService = TestContext.AddBlazoredLocalStorage();
            _ = Services.AddScoped<ILayoutService, LayoutService>();

            _ = TestContext?.AddTestAuthorization().SetAuthorized(Guid.NewGuid().ToString());

        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task CollapseButtonNavGroupShouldSaveNewState(string title, string property)
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true, CloudProvider = "Azure" });
            var cut = RenderComponent<NavMenu>();
            var navGroups = cut.FindComponents<MudNavGroup>();

            var button = navGroups.Select(c => c.Find("button")).Single(c => c.TextContent == title);

            // Act
            button.Click();

            // Assert
            var navGroupExpandedDictionary = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>(LocalStorageKey.CollapsibleNavMenu);
            Assert.IsFalse(navGroupExpandedDictionary[property]);
        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task ExpandButtonNavGroupShouldSaveState(string title, string property)
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true, CloudProvider = "Azure" });

            var dic = new Dictionary<string, bool>
            {
                { property, false }
            };
            await this.localStorageService.SetItemAsync(LocalStorageKey.CollapsibleNavMenu, dic);

            // Load layout configuration from local storage
            await Services.GetRequiredService<ILayoutService>().LoadLayoutConfigurationFromLocalStorage();

            var cut = RenderComponent<NavMenu>();
            var navGroups = cut.FindComponents<MudNavGroup>();

            var button = navGroups.Select(c => c.Find("button")).Single(c => c.TextContent == title);

            // Act
            button.Click();

            // Assert
            var navGroupExpandedDictionary = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>(LocalStorageKey.CollapsibleNavMenu);
            Assert.IsTrue(navGroupExpandedDictionary[property]);
        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task CollapseButtonNavGroupShouldSaveState(string title, string property)
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true, CloudProvider = "Azure" });
            var dic = new Dictionary<string, bool>
            {
                { property, true }
            };
            await this.localStorageService.SetItemAsync(LocalStorageKey.CollapsibleNavMenu, dic);

            // Load layout configuration from local storage
            await Services.GetRequiredService<ILayoutService>().LoadLayoutConfigurationFromLocalStorage();

            var cut = RenderComponent<NavMenu>();
            var navGroups = cut.FindComponents<MudNavGroup>();

            var button = navGroups.Select(c => c.Find("button")).Single(c => c.TextContent == title);

            // Act
            button.Click();

            // Assert
            var navGroupExpandedDictionary = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>(LocalStorageKey.CollapsibleNavMenu);
            Assert.IsFalse(navGroupExpandedDictionary[property]);
        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task WhenFalseCollapseNavGroupShouldBeCollapsed(string title, string property)
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true, CloudProvider = "Azure" });
            var dic = new Dictionary<string, bool>
            {
                { property, false }
            };

            await this.localStorageService.SetItemAsync(LocalStorageKey.CollapsibleNavMenu, dic);

            // Load layout configuration from local storage
            await Services.GetRequiredService<ILayoutService>().LoadLayoutConfigurationFromLocalStorage();

            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var navGroup = cut.FindComponents<MudNavGroup>().Single(c => c.Find("button").TextContent == title);
            Assert.IsFalse(navGroup.Instance.Expanded);
        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task WhenTrueCollapseNavGroupShouldBeExpanded(string title, string property)
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true, CloudProvider = "Azure" });
            var dic = new Dictionary<string, bool>
            {
                { property, true }
            };

            await this.localStorageService.SetItemAsync(LocalStorageKey.CollapsibleNavMenu, dic);

            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var navGroup = cut.FindComponents<MudNavGroup>().Single(c => c.Find("button").TextContent == title);
            Assert.IsTrue(navGroup.Instance.Expanded);
        }

        [Test]
        public async Task NavGroupsExpendedValuesShouldBeTrueWhenFirstTime()
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true, CloudProvider = "Azure" });

            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var navGroups = cut.FindComponents<MudNavGroup>();

            _ = navGroups.Count.Should().Be(4);
            _ = navGroups.Should().OnlyContain(navGroup => navGroup.Instance.Expanded);

            var navGroupExpandedDictionary = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>(LocalStorageKey.CollapsibleNavMenu);

            _ = navGroupExpandedDictionary.Should().BeNull();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void LoRaNavGroupShouldBeDisplayedOnlyIfSupported(bool supported)
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = supported, CloudProvider = "Azure" });

            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var navGroup = cut.FindComponents<MudNavGroup>().SingleOrDefault(c => c.Find("button").TextContent == "LoRaWAN");

            if (supported)
            {
                Assert.IsNotNull(navGroup);
            }
            else
            {
                Assert.IsNull(navGroup);
            }
        }

        [TestCase("addDeviceButton", "/devices/new")]
        [TestCase("addDeviceModelButton", "/device-models/new")]
        [TestCase("addDeviceConfigurationButton", "/device-configurations/new")]
        public void WhenClickToNewButtonShouldNavigate(string buttonName, string path)
        {
            // Arrange
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false, CloudProvider = "Azure" });
            var cut = RenderComponent<NavMenu>();
            var button = cut.WaitForElement($"#{buttonName}");

            // Act
            button.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith(path));
        }
    }
}
