// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.EdgeDevices
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Models.v10;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;

    [TestFixture]
    public class EdgeDeviceDeleteConfirmationDialogTests : TestContextWrapper, IDisposable
    {
        private MockHttpMessageHandler mockHttpClient;
        private DialogService dialogService;

        [SetUp]
        public void Setup()
        {
            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();
            this.mockHttpClient = TestContext.Services.AddMockHttpClient();
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.mockHttpClient.AutoFlush = true;

            _ = TestContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = TestContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);

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
        public async Task EdgeDeviceDeleteConfirmationDialogMustDeleteDevice()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Delete, $"/api/edge/devices/{deviceId}")
                .Respond(HttpStatusCode.NoContent);

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "DeviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<EdgeDeviceDeleteConfirmationDialog>(string.Empty, parameters));
            cut.Find("#delete").Click();
            var result = await dialogReference.GetReturnValueAsync<bool>();

            // Assert
            _ = result.Should().BeTrue();
        }

        [Test]
        public async Task EdgeDeviceDeleteConfirmationDialogShouldProcessProblemDetailsExceptionWhenIssueOccursWhenDeletingDevice()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Delete, $"/api/edge/devices/{deviceId}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "DeviceId", deviceId
                }
            };

            // Act
            await cut.InvokeAsync(() => this.dialogService?.Show<EdgeDeviceDeleteConfirmationDialog>(string.Empty, parameters));
            cut.Find("#delete").Click();

            // Assert
            _ = cut.Find("#delete").TextContent.Should().Be("Delete");
        }

        [Test]
        public async Task EdgeDeviceDeleteConfirmationDialogMustBeCanceledOnClickOnCancel()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Delete, $"/api/edge/devices/{deviceId}")
                .Respond(HttpStatusCode.NoContent);

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "DeviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<EdgeDeviceDeleteConfirmationDialog>(string.Empty, parameters));
            cut.Find("#cancel").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeTrue();
        }
    }
}
