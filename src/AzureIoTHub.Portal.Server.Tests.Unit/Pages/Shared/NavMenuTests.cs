// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Blazored.LocalStorage;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Shared;
    using FluentAssertions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;

    [TestFixture]
    public class NavMenuTests
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private Bunit.TestContext testContext;
#pragma warning restore CA2213 // Disposable fields should be disposed

        private ILocalStorageService localStorageService;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            _ = this.testContext.Services.AddMudServices();
            this.localStorageService = this.testContext.AddBlazoredLocalStorage();

            var authContext = this.testContext.AddTestAuthorization();
            _ = authContext.SetAuthorized(Guid.NewGuid().ToString());

            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task CollapseButtonNavGroupShouldSaveNewState(string title, string property)
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
            var cut = RenderComponent<NavMenu>();
            var navGroups = cut.FindComponents<MudNavGroup>();

            var button = navGroups.Select(c => c.Find("button")).Single(c => c.TextContent == title);

            // Act
            button.Click();

            // Assert
            var navGroupExpandedDictionary = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>("collapsibleNavMenu");
            Assert.IsFalse(navGroupExpandedDictionary[property]);
        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task ExpandButtonNavGroupShouldSaveState(string title, string property)
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            var dic = new Dictionary<string, bool>
            {
                { property, false }
            };
            await this.localStorageService.SetItemAsync("collapsibleNavMenu", dic);

            var cut = RenderComponent<NavMenu>();
            var navGroups = cut.FindComponents<MudNavGroup>();

            var button = navGroups.Select(c => c.Find("button")).Single(c => c.TextContent == title);

            // Act
            button.Click();

            // Assert
            var navGroupExpandedDictionary = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>("collapsibleNavMenu");
            Assert.IsTrue(navGroupExpandedDictionary[property]);
        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task CollapseButtonNavGroupShouldSaveState(string title, string property)
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
            var dic = new Dictionary<string, bool>
            {
                { property, true }
            };
            await this.localStorageService.SetItemAsync("collapsibleNavMenu", dic);

            var cut = RenderComponent<NavMenu>();
            var navGroups = cut.FindComponents<MudNavGroup>();

            var button = navGroups.Select(c => c.Find("button")).Single(c => c.TextContent == title);

            // Act
            button.Click();

            // Assert
            var navGroupExpandedDictionary = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>("collapsibleNavMenu");
            Assert.IsFalse(navGroupExpandedDictionary[property]);
        }

        [TestCase("Devices", "Devices")]
        [TestCase("IoT Edge", "IoTEdge")]
        [TestCase("LoRaWAN", "LoRaWAN")]
        [TestCase("Settings", "Settings")]
        public async Task WhenFalseCollapseNavGroupShouldBeCollapsed(string title, string property)
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
            var dic = new Dictionary<string, bool>
            {
                { property, false }
            };

            await this.localStorageService.SetItemAsync("collapsibleNavMenu", dic);

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
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
            var dic = new Dictionary<string, bool>
            {
                { property, true }
            };

            await this.localStorageService.SetItemAsync("collapsibleNavMenu", dic);

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
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = this.testContext.RenderComponent<NavMenu>();

            // Assert
            var navGroups = cut.FindComponents<MudNavGroup>();

            _ = navGroups.Count.Should().Be(4);
            _ = navGroups.Should().OnlyContain(navGroup => navGroup.Instance.Expanded);

            var navGroupExpandedDictionary = await this.localStorageService.GetItemAsync<Dictionary<string, bool>>("collapsibleNavMenu");

            _ = navGroupExpandedDictionary.Should().BeNull();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void LoRaNavGroupShouldBeDisplayedOnlyIfSupported(bool supported)
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = supported });

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

        [TestCase("addDeviceButton", "devices/new")]
        [TestCase("addDeviceModelButton", "/device-models/new")]
        [TestCase("addDeviceConfigurationButton", "/device_configurations/new")]
        public void WhenClickToNewButtonShouldNavigate(string buttonName, string path)
        {
            // Arrange
            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });
            var cut = RenderComponent<NavMenu>();
            var button = cut.WaitForElement($"#{buttonName}");

            // Act
            button.Click();

            // Assert
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith(path, StringComparison.OrdinalIgnoreCase));
        }
    }
}
