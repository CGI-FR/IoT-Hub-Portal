// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.EdgeDevices
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;
    using Models.v10;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Services;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConnectionStringDialogTests : BlazorUnitTest
    {
        private DialogService dialogService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<ClipboardService>();
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task ConnectionStringDialogMustShowEnrollmentCredentials()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{deviceId}/credentials")
                .RespondJson(new EnrollmentCredentials());

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            // Act
            await cut.InvokeAsync(() => this.dialogService?.Show<ConnectionStringDialog>(string.Empty, parameters));
            _ = cut.WaitForElement("div.mud-paper");

            // Assert
            _ = cut.FindAll("div.mud-grid-item").Count.Should().Be(4);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ConnectionStringDialogMustBeCancelledWhenProblemDetailsOccurs()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{deviceId}/credentials")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<ConnectionStringDialog>(string.Empty, parameters));
            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeTrue();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ConnectionStringDialogMustBeCancelledOnClickOnCancel()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/devices/{deviceId}/credentials")
                .RespondJson(new EnrollmentCredentials());

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<ConnectionStringDialog>(string.Empty, parameters));
            cut.WaitForElement("#cancel").Click();

            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeTrue();
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
