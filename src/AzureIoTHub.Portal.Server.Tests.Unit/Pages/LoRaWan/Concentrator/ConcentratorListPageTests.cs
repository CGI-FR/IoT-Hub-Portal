// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.LoRaWan.Concentrator
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using Models.v10.LoRaWAN;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class ConcentratorListPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
        }

        [Test]
        public void ConcentratorListPageShouldLoadAndShowConcentrators()
        {
            // Arrange

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/lorawan/concentrators?pageSize=10")
                .RespondJson(new PaginationResult<Concentrator>
                {
                    Items = new List<Concentrator>
                    {
                        new(),
                        new(),
                        new()
                    }
                });

            // Act
            var cut = RenderComponent<ConcentratorListPage>();
            cut.WaitForAssertion(() => cut.Find("#concentrators-listing"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(4);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ConcentratorListPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingConcentrators()
        {
            // Arrange

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/lorawan/concentrators?pageSize=10")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<ConcentratorListPage>();
            cut.WaitForAssertion(() => cut.Find("#concentrators-listing"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Assert
            _ = cut.FindAll("tr").Count.Should().Be(2);
            MockHttpClient.VerifyNoOutstandingRequest();
            MockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ClickToItemShouldRedirectToConcentratorDetailsPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = MockHttpClient
                .When(HttpMethod.Get, "/api/lorawan/concentrators?pageSize=10")
                .RespondJson(new PaginationResult<Concentrator>
                {
                    Items = new List<Concentrator>
                    {
                        new()
                        {
                            DeviceId = deviceId,
                        },
                        new()
                    }
                });

            var cut = RenderComponent<ConcentratorListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>()?.Uri.Should().EndWith($"/lorawan/concentrators/{deviceId}"));
        }
    }
}
