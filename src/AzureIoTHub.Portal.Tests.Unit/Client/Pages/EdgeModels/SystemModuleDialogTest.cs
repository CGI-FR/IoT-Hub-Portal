// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.EdgeModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels.EdgeModule;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using AzureIoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class SystemModuleDialogTest : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public async Task SystemModuleDialogMustCloseOnCLickOnCloseButton()
        {
            // Arrange
            var module = new EdgeModelSystemModule("edgeAgent")
            {
                ImageUri = Fixture.Create<string>(),
                EnvironmentVariables = new List<IoTEdgeModuleEnvironmentVariable>()
                {
                    new IoTEdgeModuleEnvironmentVariable()
                    {
                        Name = Fixture.Create<string>(),
                        Value = Fixture.Create<string>()
                    }
                },
                ContainerCreateOptions = Fixture.Create<string>(),
            };

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "module", module
                },
            };

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = service?.Show<SystemModuleDialog>(string.Empty, parameters));

            cut.Find("#SubmitButton").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Data.Should().Be(true);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
