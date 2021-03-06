// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Configurations.EdgeModule
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels.EdgeModule;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Bunit;
    using FluentAssertions;
    using NUnit.Framework;

    public class ModuleDialogTab1Tests : BlazorUnitTest
    {
        [Test]
        public void ModuleDialogTab1ShouldBeRenderedProperly()
        {
            //Arrange
            var module = new IoTEdgeModule()
            {
                ModuleName = Guid.NewGuid().ToString(),
                Version = Guid.NewGuid().ToString(),
                Status = "running",
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>(),
                ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSetting>(),
                Commands = new List<IoTEdgeModuleCommand>()
            };

            var cut = RenderComponent<ModuleDialogTab1>
                    (ComponentParameter.CreateParameter("EnvironmentVariables", module.EnvironmentVariables));

            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
            cut.WaitForAssertion(() => cut.Find("table tbody tr").TextContent.Should().Be("No value"));
        }

        [Test]
        public void ClickOnAddShouldAddRow()
        {
            //Arrange
            var module = new IoTEdgeModule()
            {
                ModuleName = Guid.NewGuid().ToString(),
                Version = Guid.NewGuid().ToString(),
                Status = "running",
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
                {
                    new IoTEdgeModuleEnvironmentVariable()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Value = Guid.NewGuid().ToString()
                    },
                    new IoTEdgeModuleEnvironmentVariable()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Value = Guid.NewGuid().ToString()
                    }
                },
                ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSetting>(),
                Commands = new List<IoTEdgeModuleCommand>()
            };

            var cut = RenderComponent<ModuleDialogTab1>
                    (ComponentParameter.CreateParameter("EnvironmentVariables", module.EnvironmentVariables));

            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));
            var addButton = cut.WaitForElement("#addButton");
            addButton.Click();
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(3));
        }

        [Test]
        public void ClickOnRemoveShouldDeleteRow()
        {
            //Arrange
            var module = new IoTEdgeModule()
            {
                ModuleName = Guid.NewGuid().ToString(),
                Version = Guid.NewGuid().ToString(),
                Status = "running",
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
                {
                    new IoTEdgeModuleEnvironmentVariable()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Value = Guid.NewGuid().ToString()
                    },
                    new IoTEdgeModuleEnvironmentVariable()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Value = Guid.NewGuid().ToString()
                    }
                },
                ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSetting>(),
                Commands = new List<IoTEdgeModuleCommand>()
            };

            var cut = RenderComponent<ModuleDialogTab1>
                    (ComponentParameter.CreateParameter("EnvironmentVariables", module.EnvironmentVariables));

            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));
            var removeButton = cut.WaitForElement("#removeButton");
            removeButton.Click();
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
        }
    }
}
