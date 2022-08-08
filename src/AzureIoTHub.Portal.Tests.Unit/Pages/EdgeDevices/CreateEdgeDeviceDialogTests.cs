// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.EdgeDevices
{
    using System.Threading.Tasks;
    using Models.v10;
    using Bunit;
    using Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor;
    using NUnit.Framework;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;
    using Moq;

    [TestFixture]
    public class CreateEdgeDeviceDialogTests : BlazorUnitTest
    {
        private DialogService dialogService;
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton<ClipboardService>();
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task CreateEdgeDeviceDialogMustRenderCorrectly()
        {
            // Arrange
            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters();

            // Act
            await cut.InvokeAsync(() => this.dialogService?.Show<CreateEdgeDeviceDialog>(string.Empty, parameters));
            _ = cut.WaitForElement("div.mud-paper");

            // Assert
            _ = cut.FindAll("#name").Count.Should().Be(1);
            _ = cut.FindAll("#type").Count.Should().Be(1);
            _ = cut.FindAll("#environment").Count.Should().Be(1);
            _ = cut.FindAll("#cancel").Count.Should().Be(1);
            _ = cut.FindAll("#create").Count.Should().Be(1);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task CreateEdgeDeviceDialogMustBeCancelledOnClickOnCancel()
        {
            // Arrange
            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters();

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<CreateEdgeDeviceDialog>(string.Empty, parameters));
            _ = cut.WaitForElement("div.mud-paper");
            cut.Find("#cancel").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeTrue();
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
