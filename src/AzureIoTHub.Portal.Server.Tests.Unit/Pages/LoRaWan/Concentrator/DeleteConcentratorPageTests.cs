// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.LoRaWan.Concentrator
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
    using Models.v10;
    using Helpers;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeleteConcentratorPageTests : TestContextWrapper, IDisposable
    {
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private DialogService dialogService;
        private Mock<ISnackbar> mockSnackbarService;

        [SetUp]
        public void SetUp()
        {
            TestContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = TestContext.Services.AddMockHttpClient();

            this.mockSnackbarService = this.mockRepository.Create<ISnackbar>();
            _ = TestContext.Services.AddSingleton(this.mockSnackbarService.Object);

            _ = TestContext.Services.AddMudServices();
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
            this.mockHttpClient.AutoFlush = true;

            this.dialogService = TestContext.Services.GetService<IDialogService>() as DialogService;
        }

        [TearDown]
        public void TearDown() => TestContext?.Dispose();

        [Test]
        public async Task DeleteConcentratorPageShouldDeleteConcentrator()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Delete, $"/api/lorawan/concentrators/{deviceId}")
                .RespondText(string.Empty);

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);

            IDialogReference dialogReference = null;

            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<DeleteConcentratorPage>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#delete-concentrator"));

            // Act
            cut.Find("#delete-concentrator").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeFalse();
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public async Task DeleteConcentratorPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnDeletingConcentrator()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockHttpClient
                .When(HttpMethod.Delete, $"/api/lorawan/concentrators/{deviceId}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "deviceId", deviceId
                }
            };

            await cut.InvokeAsync(() => this.dialogService?.Show<DeleteConcentratorPage>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#delete-concentrator"));

            // Act
            cut.Find("#delete-concentrator").Click();

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
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
