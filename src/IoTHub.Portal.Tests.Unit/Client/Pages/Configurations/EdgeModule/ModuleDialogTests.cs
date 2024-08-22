// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Configurations.EdgeModule
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Dialogs.EdgeModels.EdgeModule;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using MudBlazor;
    using NUnit.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class ModuleDialogTests : BlazorUnitTest
    {

        [Test]
        public async Task ModuleDialogTestMustBeRenderedOnShow()
        {
            //Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            var moduleName = Guid.NewGuid().ToString();
            var moduleImageUri = Guid.NewGuid().ToString();

            var module = new IoTEdgeModule()
            {
                ModuleName = moduleName,
                Status = "running",
                ImageURI = moduleImageUri,
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>(),
                ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSetting>(),
                Commands = new List<IoTEdgeModuleCommand>()
            };

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "module", module
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<ModuleDialog>(string.Empty, parameters));

            cut.WaitForAssertion(() => cut.Find("div.mud-dialog-container").Should().NotBeNull());
            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeModule.ModuleName)}").OuterHtml.Should().Contain(moduleName));
            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeModule.ImageURI)}").OuterHtml.Should().Contain(moduleImageUri));

            // Assert
            var tabs = cut.WaitForElements(".mud-tabs .mud-tab");
            Assert.AreEqual(3, tabs.Count);
            Assert.AreEqual("Environment variables", tabs[0].TextContent);
            Assert.AreEqual("Module identity twin settings", tabs[1].TextContent);
            Assert.AreEqual("Commands", tabs[2].TextContent);
        }

        [Test]
        public async Task ClickOnSubmitShouldUpdateModuleValues()
        {
            //Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            var moduleName = Guid.NewGuid().ToString();
            var moduleImageUri = Guid.NewGuid().ToString();

            var module = new IoTEdgeModule()
            {
                ModuleName = moduleName,
                Status = "running",
                ImageURI = moduleImageUri,
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>(),
                ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSetting>(),
                Commands = new List<IoTEdgeModuleCommand>()
            };

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "module", module
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<ModuleDialog>(string.Empty, parameters));

            cut.WaitForAssertion(() => cut.Find("div.mud-dialog-container").Should().NotBeNull());

            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeModule.ModuleName)}").Change("newModuleNameValue"));
            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeModule.ImageURI)}").Change("newModuleImageUriValue"));

            var submitButton = cut.WaitForElement("#SubmitButton");
            submitButton.Click();

            cut.WaitForAssertion(() => module.ModuleName.Should().Be("newModuleNameValue"));
            cut.WaitForAssertion(() => module.ImageURI.Should().Be("newModuleImageUriValue"));
        }

        /*============ For AWS ======================*/

        [Test]
        public async Task ForAWSModuleDialogTestMustBeRenderedOnShow()
        {
            //Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            var moduleName = Guid.NewGuid().ToString();
            var moduleImageUri = Guid.NewGuid().ToString();

            var module = new IoTEdgeModule()
            {
                ModuleName = moduleName,
                Status = "running",
                ImageURI = moduleImageUri,
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
            };

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "module", module
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<ModuleDialog>(string.Empty, parameters));

            cut.WaitForAssertion(() => cut.Find("div.mud-dialog-container").Should().NotBeNull());
            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeModule.ModuleName)}").OuterHtml.Should().Contain(moduleName));
            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeModule.ImageURI)}").OuterHtml.Should().Contain(moduleImageUri));

            // Assert
            var tabs = cut.WaitForElements(".mud-tabs .mud-tab");
            Assert.AreEqual(1, tabs.Count);
            Assert.AreEqual("Environment variables", tabs[0].TextContent);

        }

        [Test]
        public async Task ForAWSClickOnSubmitShouldUpdateModuleValues()
        {
            //Arrange
            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            var moduleName = Guid.NewGuid().ToString();
            var moduleImageUri = Guid.NewGuid().ToString();

            var module = new IoTEdgeModule()
            {
                ModuleName = moduleName,
                Status = "running",
                ImageURI = moduleImageUri,
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
            };

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "module", module
                }
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<ModuleDialog>(string.Empty, parameters));

            cut.WaitForAssertion(() => cut.Find("div.mud-dialog-container").Should().NotBeNull());

            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeModule.ModuleName)}").Change("newModuleNameValue"));
            cut.WaitForAssertion(() => cut.Find($"#{nameof(IoTEdgeModule.ImageURI)}").Change("newModuleImageUriValue"));

            var submitButton = cut.WaitForElement("#SubmitButton");
            submitButton.Click();

            cut.WaitForAssertion(() => module.ModuleName.Should().Be("newModuleNameValue"));
            cut.WaitForAssertion(() => module.ImageURI.Should().Be("newModuleImageUriValue"));
        }
    }
}
