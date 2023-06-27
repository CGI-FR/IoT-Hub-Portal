// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Configurations.EdgeModule
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Client.Pages.EdgeModels.EdgeModule;
    using Models.v10;
    using IoTHub.Portal.Shared.Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using NUnit.Framework;

    public class ModuleDialogTab3Tests : BlazorUnitTest
    {
        [Test]
        public void ModuleDialogTab3ShouldBeRenderedProperly()
        {
            //Arrange
            var module = new IoTEdgeModuleDto()
            {
                ModuleName = Guid.NewGuid().ToString(),
                Status = "running",
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariableDto>(),
                ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSettingDto>(),
                Commands = new List<IoTEdgeModuleCommandDto>()
            };

            var cut = RenderComponent<ModuleDialogTab3>
                    (ComponentParameter.CreateParameter("Commands", module.Commands));

            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
            cut.WaitForAssertion(() => cut.Find("table tbody tr").TextContent.Should().Be("No value"));
        }

        [Test]
        public void ClickOnAddShouldAddRow()
        {
            //Arrange
            var module = new IoTEdgeModuleDto()
            {
                ModuleName = Guid.NewGuid().ToString(),
                Status = "running",
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariableDto>(),
                ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSettingDto>(),
                Commands = new List<IoTEdgeModuleCommandDto>()
                {
                    new IoTEdgeModuleCommandDto()
                    {
                        Name = Guid.NewGuid().ToString()
                    },
                    new IoTEdgeModuleCommandDto()
                    {
                        Name = Guid.NewGuid().ToString()
                    }
                }
            };

            var cut = RenderComponent<ModuleDialogTab3>
                    (ComponentParameter.CreateParameter("Commands", module.Commands));

            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));
            var addButton = cut.WaitForElement("#addButton");
            addButton.Click();
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(3));
        }

        [Test]
        public void ClickOnRemoveShouldDeleteRow()
        {
            //Arrange
            var module = new IoTEdgeModuleDto()
            {
                ModuleName = Guid.NewGuid().ToString(),
                Status = "running",
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariableDto>(),
                ModuleIdentityTwinSettings = new List<IoTEdgeModuleTwinSettingDto>(),
                Commands = new List<IoTEdgeModuleCommandDto>()
                {
                    new IoTEdgeModuleCommandDto()
                    {
                        Name = Guid.NewGuid().ToString()
                    },
                    new IoTEdgeModuleCommandDto()
                    {
                        Name = Guid.NewGuid().ToString()
                    }
                }
            };

            var cut = RenderComponent<ModuleDialogTab3>
                    (ComponentParameter.CreateParameter("Commands", module.Commands));

            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(2));
            var removeButton = cut.WaitForElement("#removeButton");
            removeButton.Click();
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
        }
    }
}
