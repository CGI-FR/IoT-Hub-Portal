// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.LoRaWan.Concentrator
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.LoRaWAN.Concentrator;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using AutoFixture;
    using Portal.Shared;
    using Portal.Shared.Models.v1._0;
    using Portal.Shared.Models.v1._0.LoRaWAN;

    [TestFixture]
    public class ConcentratorListPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<ILoRaWanConcentratorClientService> mockLoRaWanConcentratorClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockLoRaWanConcentratorClientService = MockRepository.Create<ILoRaWanConcentratorClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanConcentratorClientService.Object);

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });
        }

        [Test]
        public void ConcentratorListPageShouldLoadAndShowConcentrators()
        {
            // Arrange
            var expectedUri = "api/lorawan/concentrators?pageNumber=0&pageSize=10&orderBy=&searchText=&status=&state=";

            _ = this.mockLoRaWanConcentratorClientService.Setup(service =>
                    service.GetConcentrators(It.Is<string>(s => expectedUri.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new PaginationResult<ConcentratorDto>
                {
                    Items = new List<ConcentratorDto>
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
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ConcentratorListPageShouldProcessProblemDetailsExceptionWhenIssueOccursOnLoadingConcentrators()
        {
            // Arrange
            var expectedUri = "api/lorawan/concentrators?pageNumber=0&pageSize=10&orderBy=&searchText=&status=&state=";

            _ = this.mockLoRaWanConcentratorClientService.Setup(service =>
                    service.GetConcentrators(It.Is<string>(s => expectedUri.Equals(s, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

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
            var expectedUri = "api/lorawan/concentrators?pageNumber=0&pageSize=10&orderBy=&searchText=&status=&state=";

            _ = this.mockLoRaWanConcentratorClientService.Setup(service =>
                    service.GetConcentrators(It.Is<string>(s => expectedUri.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new PaginationResult<ConcentratorDto>
                {
                    Items = new List<ConcentratorDto>
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

        [Test]
        public void ClickOnAddNewDeviceShouldNavigateToNewDevicePage()
        {
            // Arrange
            const string expectedUri = "api/lorawan/concentrators?pageNumber=0&pageSize=10&orderBy=&searchText=&status=&state=";

            _ = this.mockLoRaWanConcentratorClientService.Setup(service =>
                    service.GetConcentrators(It.Is<string>(s => expectedUri.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new PaginationResult<ConcentratorDto>
                {
                    Items = new List<ConcentratorDto>
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
            cut.WaitForElement("#add-concentrator").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>()?.Uri.Should().EndWith("/lorawan/concentrators/new"));
        }

        [Test]
        public void ClickOnRefreshShouldReloadConcentrators()
        {
            // Arrange
            var expectedUri = "api/lorawan/concentrators?pageNumber=0&pageSize=10&orderBy=&searchText=&status=&state=";

            _ = this.mockLoRaWanConcentratorClientService.Setup(service =>
                    service.GetConcentrators(It.Is<string>(s => expectedUri.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new PaginationResult<ConcentratorDto>
                {
                    Items = Array.Empty<ConcentratorDto>()
                });

            var cut = RenderComponent<ConcentratorListPage>();

            // Act
            cut.WaitForElement("#tableRefreshButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSearchShouldSearchConcentrators()
        {
            // Arrange
            var searchKeyword = Fixture.Create<string>();
            var concentratorSearchInfo = new ConcentratorSearchInfo
            {
                SearchText = searchKeyword,
                State = string.Empty,
                Status = string.Empty
            };
            var expectedUri = $"api/lorawan/concentrators?pageNumber=0&pageSize=10&orderBy=&searchText=&status=&state=";

            _ = this.mockLoRaWanConcentratorClientService.Setup(service =>
                    service.GetConcentrators(It.Is<string>(s => expectedUri.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new PaginationResult<ConcentratorDto>
                {
                    Items = new List<ConcentratorDto>()
                    {
                        new (),
                        new (),
                        new (),
                    }
                });

            var expectedUriWithFilter = $"api/lorawan/concentrators?pageNumber=0&pageSize=10&orderBy=&searchText={concentratorSearchInfo.SearchText}&status={concentratorSearchInfo.Status}&state={concentratorSearchInfo.State}";

            _ = this.mockLoRaWanConcentratorClientService.Setup(service =>
                    service.GetConcentrators(It.Is<string>(s => expectedUriWithFilter.Equals(s, StringComparison.Ordinal))))
                .ReturnsAsync(new PaginationResult<ConcentratorDto>
                {
                    Items = new List<ConcentratorDto>()
                    {
                        new ()
                    }
                });

            var cut = RenderComponent<ConcentratorListPage>();

            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(3));
            cut.WaitForElement("#searchKeyword").Change(searchKeyword);

            // Act
            cut.WaitForElement("#searchButton").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
