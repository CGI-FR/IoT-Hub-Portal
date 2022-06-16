// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Edge_Devices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.Edge_Devices;
    using Models.v10;
    using Bunit;
    using Client.Exceptions;
    using Client.Models;
    using Client.Services;
    using FluentAssertions;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JSInterop;
    using Moq;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class EdgeDeviceListPageTests : TestContextWrapper, IDisposable
    {
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IJSRuntime> mockJsRuntime;

        [SetUp]
        public void Setup()
        {
            TestContext = new Bunit.TestContext();
            _ = TestContext.Services.AddMudServices();
            this.mockHttpClient = TestContext.Services.AddMockHttpClient();
            _ = TestContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockJsRuntime = this.mockRepository.Create<IJSRuntime>();
            _ = TestContext.Services.AddSingleton(new ClipboardService(this.mockJsRuntime.Object));

            this.mockHttpClient.AutoFlush = true;

            _ = TestContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = TestContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = TestContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
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
        public void EdgeDeviceListPageShouldShowEdgeDevices()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/edge/devices?pageSize=10&searchText=&searchStatus=&searchType=")
                .RespondJson(new PaginationResult<IoTEdgeListItem>
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        new(),
                        new(),
                        new()
                    }
                });

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("tr").Count.Should().Be(4);
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void EdgeDeviceListPageShouldShowNoContentWhenNoEdgeDevices()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/edge/devices?pageSize=10&searchText=&searchStatus=&searchType=")
                .RespondJson(new PaginationResult<IoTEdgeListItem>());

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("tr").Count.Should().Be(2);
            _ = cut.Markup.Should().Contain("No matching records found");
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void EdgeDeviceListPageShouldShowNoContentWhenProblemDetailsExceptionOccurs()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/edge/devices?pageSize=10&searchText=&searchStatus=&searchType=")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("tr").Count.Should().Be(2);
            _ = cut.Markup.Should().Contain("No matching records found");
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void EdgeDeviceListPageShouldResetOnClickOnReset()
        {
            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, "/api/edge/devices?pageSize=10&searchText=&searchStatus=&searchType=")
                .RespondJson(new PaginationResult<IoTEdgeListItem>
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        new(),
                        new(),
                        new()
                    }
                });

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();
            cut.Find("#reset").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("tr").Count.Should().Be(4);
            this.mockHttpClient.VerifyNoOutstandingRequest();
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }
    }
}
