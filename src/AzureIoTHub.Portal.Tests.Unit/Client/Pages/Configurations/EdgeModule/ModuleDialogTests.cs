// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Configurations.EdgeModule
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels.EdgeModule;
    using Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class ModuleDialogTests : BlazorUnitTest
    {

        [Test]
        public async Task ModuleDialogTestMustBeRenderedOnShow()
        {
            //Arrange
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
            await cut.InvokeAsync(() => service?.Show<ModuleDialog>(string.Empty, parameters));

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
            await cut.InvokeAsync(() => service?.Show<ModuleDialog>(string.Empty, parameters));

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
