// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DeviceConfigurations
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Models.v10;
    using Helpers;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Pages.DeviceConfigurations;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeleteDeviceConfigurationTests : BlazorUnitTest
    {
        private DialogService dialogService;
        private Mock<ISnackbar> mockSnackbarService;

        public override void Setup()
        {
            base.Setup();

            this.mockSnackbarService = MockRepository.Create<ISnackbar>();

            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            this.dialogService = Services.GetService<IDialogService>() as DialogService;
        }

        [Test]
        public async Task DeleteDeviceConfigurationShouldDeleteConfiguration()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var configurationName = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Delete, $"/api/device-configurations/{configurationId}")
                .RespondText(string.Empty);

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "configurationId", configurationId
                },
                {
                    "configurationName", configurationName
                }
            };

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);

            IDialogReference dialogReference = null;

            await cut.InvokeAsync(() => dialogReference = this.dialogService?.Show<DeleteDeviceConfiguration>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#delete-device-configuration"));

            // Act
            cut.Find("#delete-device-configuration").Click();
            var result = await dialogReference.Result;

            // Assert
            _ = result.Cancelled.Should().BeFalse();
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task DeleteConcentratorPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnDeletingConcentrator()
        {
            // Arrange
            var configurationId = Guid.NewGuid().ToString();
            var configurationName = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Delete, $"/api/device-configurations/{configurationId}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<MudDialogProvider>();

            var parameters = new DialogParameters
            {
                {
                    "configurationId", configurationId
                },
                {
                    "configurationName", configurationName
                }
            };

            await cut.InvokeAsync(() => this.dialogService?.Show<DeleteDeviceConfiguration>(string.Empty, parameters));
            cut.WaitForAssertion(() => cut.Find("#delete-device-configuration"));

            // Act
            cut.Find("#delete-device-configuration").Click();

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }
    }
}
