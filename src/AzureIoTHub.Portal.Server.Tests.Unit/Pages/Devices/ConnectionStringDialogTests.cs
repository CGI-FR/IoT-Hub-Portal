// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Pages.Devices;
    using Client.Services;
    using FluentAssertions;
    using Helpers;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JSInterop;
    using Models.v10;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConnectionStringDialogTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;

        private MockRepository mockRepository;
        private Mock<IJSRuntime> mockJSRuntime;

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockJSRuntime = this.mockRepository.Create<IJSRuntime>();
            this.mockHttpClient = this.testContext.Services
                .AddMockHttpClient();

            _ = this.testContext.Services.AddSingleton(new ClipboardService(this.mockJSRuntime.Object));

            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudElementRef.saveFocus", _ => true);

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public async Task ConnectionStringDialogMustBeRenderedOnShow()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/devices/{deviceId}/credentials")
                .RespondJson(new EnrollmentCredentials());

            var cut = RenderComponent<MudDialogProvider>();
            var service = this.testContext.Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            // Act
            await cut.InvokeAsync(() => service?.Show<ConnectionStringDialog>(string.Empty, parameters));

            // Assert
            cut.WaitForAssertion(() => cut.Find("div.mud-dialog-container").Should().NotBeNull());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public async Task OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingCredentials()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/devices/{deviceId}/credentials")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();
            var service = this.testContext.Services.GetService<IDialogService>() as DialogService;

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            IDialogReference dialogReference = null;

            // Act
            await cut.InvokeAsync(() => dialogReference = service?.Show<ConnectionStringDialog>(string.Empty, parameters));

            var result = await dialogReference.Result;

            // Assert
            cut.WaitForAssertion(() => result.Cancelled.Should().BeFalse());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
