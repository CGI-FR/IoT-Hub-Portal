// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using AzureIoTHub.Portal.Client.Pages.Configurations;
    using Models.v10;
    using Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConfigDetailTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private readonly string mockConfigurationID = Guid.NewGuid().ToString();

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();

            this.mockHttpClient = this.testContext.Services.AddMockHttpClient();

            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);
            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        [TestCase]
        public void ReturnButtonMustNavigateToPreviousPage()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/configurations/{this.mockConfigurationID}")
                .RespondJson(new ConfigListItem());

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            var cut = RenderComponent<ConfigDetail>(ComponentParameter.CreateParameter("ConfigurationID", this.mockConfigurationID));
            Thread.Sleep(500);

            var returnButton = cut.WaitForElement("#returnButton");

            var mockNavigationManager = this.testContext.Services.GetRequiredService<FakeNavigationManager>();

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForState(() => mockNavigationManager.Uri.EndsWith("/edge/configurations", StringComparison.OrdinalIgnoreCase));
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [TestCase]
        public void ConfigDetailShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingConfiguration()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/edge/configurations/{this.mockConfigurationID}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });


            // Act
            var cut = RenderComponent<ConfigDetail>(ComponentParameter.CreateParameter("ConfigurationID", this.mockConfigurationID));

            // Assert
            _ = cut.Markup.Should().NotBeEmpty();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
