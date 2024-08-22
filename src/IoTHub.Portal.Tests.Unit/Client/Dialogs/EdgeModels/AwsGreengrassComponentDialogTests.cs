// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Dialogs.EdgeModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Dialogs.EdgeModels;
    using IoTHub.Portal.Client.Enums;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    [TestFixture]
    public class AwsGreengrassComponentDialogTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public async Task AwsGreengrassComponentDialog_ClickOnCancel_DialogCanceled()
        {
            // Arrange
            var edgeModules = Array.Empty<IoTEdgeModule>().ToList();

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                { "Context", Context.Create },
                { "EdgeModules", edgeModules }
            };

            IDialogReference dialogReference = null;
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<AwsGreengrassComponentDialog>(string.Empty, parameters));

            // Act
            cut.Find("#greengrass-component-cancel").Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeTrue();
            _ = edgeModules.Should().BeEmpty();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task AwsGreengrassComponentDialog_CreateAWSComponentAndSubmit_EdgeModuleAdded()
        {
            // Arrange
            var edgeModules = Array.Empty<IoTEdgeModule>().ToList();

            var inputJsonRecipe = /*lang=json*/ @"
{
  ""ComponentName"": ""com.example.DDboxAdvantech"",
  ""ComponentVersion"": ""1.0.0""
}
";

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                { "Context", Context.Create },
                { "EdgeModules", edgeModules }
            };

            IDialogReference dialogReference = null;
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<AwsGreengrassComponentDialog>(string.Empty, parameters));

            // Act
            cut.WaitForElement("#greengrass-component-recipe-json").Change(inputJsonRecipe);
            cut.Find("#greengrass-component-submit").Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeFalse();
            _ = edgeModules.Count.Should().Be(1);
            _ = edgeModules.First().ModuleName.Should().Be("com.example.DDboxAdvantech");
            _ = edgeModules.First().Version.Should().Be("1.0.0");
            _ = edgeModules.First().ContainerCreateOptions.Should().Be(inputJsonRecipe);
            _ = edgeModules.First().ImageURI.Should().Be("example.com");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task AwsGreengrassComponentDialog_EditAWSComponentAndSubmit_EdgeModuleUpdated()
        {
            // Arrange
            var existingJsonRecipe = /*lang=json*/ @"
{
  ""ComponentName"": ""com.example.DDboxAdvantech"",
  ""ComponentVersion"": ""1.0.0""
}
";
            var edgeModule = new IoTEdgeModule
            {
                ModuleName = "com.example.DDboxAdvantech",
                Version = "1.0.0",
                ContainerCreateOptions = existingJsonRecipe
            };

            var newJsonRecipe = /*lang=json*/ @"
{
  ""ComponentName"": ""com.example.DDboxAdvantech"",
  ""ComponentVersion"": ""2.0.0""
}
";

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                { "Context", Context.Edit },
                { "Module", edgeModule }
            };

            IDialogReference dialogReference = null;
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<AwsGreengrassComponentDialog>(string.Empty, parameters));

            // Act
            cut.WaitForElement("#greengrass-component-recipe-json").Change(newJsonRecipe);
            cut.Find("#greengrass-component-submit").Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeFalse();
            _ = edgeModule.ModuleName.Should().Be("com.example.DDboxAdvantech");
            _ = edgeModule.Version.Should().Be("2.0.0");
            _ = edgeModule.ContainerCreateOptions.Should().Be(newJsonRecipe);
            _ = edgeModule.ImageURI.Should().Be("example.com");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
