// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Configurations
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using Models.v10;
    using Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;
    using AzureIoTHub.Portal.Client.Pages.EdgeModels;

    [TestFixture]
    public class ConfigDetailTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private readonly string mockConfigurationId = Guid.NewGuid().ToString();

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
        }

        [TestCase]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/configurations/{this.mockConfigurationId}")
                .RespondJson(new ConfigListItem());

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            var cut = RenderComponent<ConfigDetail>(ComponentParameter.CreateParameter("ConfigurationID", this.mockConfigurationId));
            Thread.Sleep(500);

            var returnButton = cut.WaitForElement("#returnButton");

            var mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForState(() => mockNavigationManager.Uri.EndsWith("/edge/configurations", StringComparison.OrdinalIgnoreCase));
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [TestCase]
        public void ConfigDetailShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConfiguration()
        {
            // Arrange
            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/edge/configurations/{this.mockConfigurationId}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });


            // Act
            var cut = RenderComponent<ConfigDetail>(ComponentParameter.CreateParameter("ConfigurationID", this.mockConfigurationId));

            // Assert
            _ = cut.Markup.Should().NotBeEmpty();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
