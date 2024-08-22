// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Dialogs.EdgeModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Dialogs.EdgeModels;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Shared.Models.v1._0;

    [TestFixture]
    public class AwsGreengrassPublicComponentsDialogTests : BlazorUnitTest
    {
        private Mock<IEdgeModelClientService> mockEdgeModelClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeModelClientService = MockRepository.Create<IEdgeModelClientService>();

            _ = Services.AddSingleton(this.mockEdgeModelClientService.Object);
        }

        [Test]
        public async Task AwsGreengrassPublicComponentsDialog_AfterOnInitializedAsync_PublicEdgeComponentsAreLoaded()
        {
            // Arrange
            var edgeModules = Array.Empty<IoTEdgeModule>().ToList();
            var publicEdgeComponents = Fixture.CreateMany<IoTEdgeModule>(10).ToList();

            _ = this.mockEdgeModelClientService.Setup(s => s.GetPublicEdgeModules())
                .ReturnsAsync(publicEdgeComponents);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"EdgeModules", edgeModules}
            };

            // Act
            _ = await cut.InvokeAsync(() => service?.Show<AwsGreengrassPublicComponentsDialog>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(10));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task AwsGreengrassPublicComponentsDialog_ClickOnCancel_DialogCanceled()
        {
            // Arrange
            var edgeModules = Array.Empty<IoTEdgeModule>().ToList();
            var publicEdgeComponents = Fixture.CreateMany<IoTEdgeModule>(10).ToList();

            _ = this.mockEdgeModelClientService.Setup(s => s.GetPublicEdgeModules())
                .ReturnsAsync(publicEdgeComponents);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"EdgeModules", edgeModules}
            };

            IDialogReference dialogReference = null;
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<AwsGreengrassPublicComponentsDialog>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(10));

            // Act
            cut.Find("#greengrass-public-components-cancel").Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeTrue();
            _ = edgeModules.Should().BeEmpty();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task AwsGreengrassPublicComponentsDialog_ClickOnSubmitWithoutSelectingPublicComponents_EdgeModulesNotUpdated()
        {
            // Arrange
            var edgeModules = Array.Empty<IoTEdgeModule>().ToList();
            var publicEdgeComponents = Fixture.CreateMany<IoTEdgeModule>(10).ToList();

            _ = this.mockEdgeModelClientService.Setup(s => s.GetPublicEdgeModules())
                .ReturnsAsync(publicEdgeComponents);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"EdgeModules", edgeModules}
            };

            IDialogReference dialogReference = null;
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<AwsGreengrassPublicComponentsDialog>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(10));

            // Act
            cut.Find("#greengrass-public-components-submit").Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeFalse();
            _ = edgeModules.Should().BeEmpty();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task AwsGreengrassPublicComponentsDialog_ClickOnSubmitAfterSelectingPublicComponent_EdgeModulesNotUpdated()
        {
            // Arrange
            var edgeModules = Array.Empty<IoTEdgeModule>().ToList();
            var publicEdgeComponents = Fixture.CreateMany<IoTEdgeModule>(10).ToList();

            _ = this.mockEdgeModelClientService.Setup(s => s.GetPublicEdgeModules())
                .ReturnsAsync(publicEdgeComponents);

            var cut = RenderComponent<MudDialogProvider>();
            var service = Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {"EdgeModules", edgeModules}
            };

            IDialogReference dialogReference = null;
            _ = await cut.InvokeAsync(() => dialogReference = service?.Show<AwsGreengrassPublicComponentsDialog>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(10));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());
            cut.Find("#greengrass-public-components-submit").Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Canceled.Should().BeFalse();
            _ = edgeModules.Count.Should().Be(1);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
