// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.EdgeDevices
{
    using System;
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.EdgeDevices;
    using AzureIoTHub.Portal.Client.Services;
    using Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class EdgeDeviceListPageTests : BlazorUnitTest
    {
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });
            _ = Services.AddSingleton<ClipboardService>();
        }

        [Test]
        public void EdgeDeviceListPageShouldShowEdgeDevices()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>
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
            _ = cut.FindAll("table tbody tr").Count.Should().Be(3);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeDeviceListPageShouldShowNoContentWhenNoEdgeDevices()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>());

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("table tbody tr").Count.Should().Be(1);
            _ = cut.Markup.Should().Contain("No matching records found");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeDeviceListPageShouldShowNoContentWhenProblemDetailsExceptionOccurs()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("table tbody tr").Count.Should().Be(1);
            _ = cut.Markup.Should().Contain("No matching records found");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeDeviceListPageShouldResetOnClickOnReset()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        new(),
                        new(),
                        new()
                    }
                });

            var cut = RenderComponent<EdgeDeviceListPage>();
            cut.WaitForAssertion(() => cut.Find("#reset"));

            // Act
            cut.Find("#reset").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("table tbody tr").Count.Should().Be(3);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickToItemShouldRedirectToEdgeDetailsPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        new()
                        {
                            DeviceId = deviceId,
                        },
                        new()
                    }
                });

            var cut = RenderComponent<EdgeDeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>()?.Uri.Should().EndWith($"/edge/devices/{deviceId}"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnRefreshShouldReloadEdgeDevices()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>
                {
                    Items = Array.Empty<IoTEdgeListItem>()
                });

            var cut = RenderComponent<EdgeDeviceListPage>();

            // Act
            cut.WaitForElement("#tableRefreshButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
