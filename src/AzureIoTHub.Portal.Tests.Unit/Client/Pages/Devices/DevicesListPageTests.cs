// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Client.Services;
    using Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class DevicesListPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;
        private Mock<IDeviceClientService> mockDeviceClientService;

        private readonly string apiBaseUrl = "api/devices";

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();
            this.mockDeviceClientService = MockRepository.Create<IDeviceClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
            _ = Services.AddSingleton(this.mockDeviceClientService.Object);
        }

        [Test]
        public void DeviceListPageRendersCorrectly()
        {
            // Arrange
            using var deviceResponseMock = new HttpResponseMessage();

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Find(".mud-expansion-panels .mud-expand-panel .mud-expand-panel-header .mud-expand-panel-text").TextContent.Should().Be("Search panel"));
            cut.WaitForAssertion(() => cut.Find(".mud-expansion-panels .mud-expand-panel").ClassList.Should().NotContain("Search panel should be collapsed"));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task WhenResetFilterButtonClickShouldClearFilters()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());


            // Act
            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForElement("#searchID").NodeValue = Guid.NewGuid().ToString();
            cut.WaitForElement("#searchStatusEnabled").Click();
            cut.WaitForElement("#searchStateDisconnected").Click();

            cut.WaitForElement("#resetSearch").Click();
            await Task.Delay(100);

            // Assert
            cut.WaitForAssertion(() => Assert.IsNull(cut.Find("#searchID").NodeValue));
            cut.WaitForAssertion(() => Assert.AreEqual("false", cut.Find("#searchStatusEnabled").Attributes["aria-checked"].Value));
            cut.WaitForAssertion(() => Assert.AreEqual("false", cut.Find("#searchStateDisconnected").Attributes["aria-checked"].Value));
            cut.WaitForAssertion(() => Assert.AreEqual("true", cut.Find("#searchStatusAll").Attributes["aria-checked"].Value));
            cut.WaitForAssertion(() => Assert.AreEqual("true", cut.Find("#searchStateAll").Attributes["aria-checked"].Value));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenAddNewDeviceClickShouldNavigateToNewDevicePage()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();


            // Act
            var cut = RenderComponent<DeviceListPage>();

            cut.WaitForElement("#addDeviceButton").Click();
            cut.WaitForAssertion(() => string.Equals("http://localhost/devices/new", mockNavigationManager.Uri, StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnRefreshShouldReloadDevices()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceListPage>();

            // Act
            cut.WaitForElement("#tableRefreshButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureDisableClickToItemShouldRedirectToDeviceDetailsPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId } }
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/devices/{deviceId}"));
        }

        [Test]
        public void WhenLoraFeatureEnableClickToItemShouldRedirectToLoRaDeviceDetailsPage()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();

            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = new[] { new DeviceListItem { DeviceID = deviceId, SupportLoRaFeatures = true } }
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            var cut = RenderComponent<DeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForAssertion(() => cut.Find("table tbody tr").Click());

            // Assert
            cut.WaitForAssertion(() => Services.GetService<FakeNavigationManager>().Uri.Should().EndWith($"/devices/{deviceId}?isLora=true"));
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceTags()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState="))
                .ReturnsAsync(new PaginationResult<DeviceListItem>
                {
                    Items = Array.Empty<DeviceListItem>()
                });

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void LoadItemsShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDevices()
        {
            // Arrange
            _ = this.mockDeviceClientService.Setup(service =>
                    service.GetDevices($"{this.apiBaseUrl}?pageSize=10&searchText=&searchStatus=&searchState="))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = true });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>());

            // Act
            var cut = RenderComponent<DeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => cut.FindAll("tr").Count.Should().Be(2));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
