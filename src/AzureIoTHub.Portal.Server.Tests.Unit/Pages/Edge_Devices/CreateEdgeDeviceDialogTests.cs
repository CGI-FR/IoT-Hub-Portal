// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Edge_Devices
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.Edge_Devices;
    using Models.v10;
    using Bunit;
    using Client.Services;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JSInterop;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateEdgeDeviceDialogTests : TestContextWrapper, IDisposable
    {
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IJSRuntime> mockJsRuntime;
        private DialogService dialogService;

        [SetUp]
        public void Setup()
        {
            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();
            this.mockHttpClient = TestContext.Services.AddMockHttpClient();
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockJsRuntime = this.mockRepository.Create<IJSRuntime>();
            _ = TestContext.Services.AddSingleton(new ClipboardService(this.mockJsRuntime.Object));

            this.mockHttpClient.AutoFlush = true;

            _ = TestContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = TestContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = TestContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = TestContext.JSInterop.SetupVoid("mudElementRef.saveFocus", _ => true);

            this.dialogService = TestContext.Services.GetService<IDialogService>() as DialogService;
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
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
        }
    }
}
