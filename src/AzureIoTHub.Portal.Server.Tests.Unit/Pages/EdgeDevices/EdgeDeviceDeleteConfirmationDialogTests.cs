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
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;

    [TestFixture]
    public class EdgeDeviceDeleteConfirmationDialogTests : BlazorUnitTest
    {
        private DialogService dialogService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task EdgeDeviceDeleteConfirmationDialogMustDeleteDevice()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = MockHttpClient
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

            _ = MockHttpClient
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

            _ = MockHttpClient
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
