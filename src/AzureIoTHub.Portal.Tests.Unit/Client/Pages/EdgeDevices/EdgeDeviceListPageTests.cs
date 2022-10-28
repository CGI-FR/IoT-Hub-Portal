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
    using System.Threading.Tasks;
    using System.Linq;

    [TestFixture]
    public class EdgeDeviceListPageTests : BlazorUnitTest
    {
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;
        private Mock<IEdgeModelClientService> mockEdgeModelClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();
            this.mockEdgeModelClientService = MockRepository.Create<IEdgeModelClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockEdgeModelClientService.Object);
            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });
            _ = Services.AddSingleton<ClipboardService>();
        }

        [Test]
        public void EdgeDeviceListPageShouldShowEdgeDevices()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
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

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Description = Guid.NewGuid().ToString(),
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
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>());

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString()
                    }
                });

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
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString()
                    }
                });

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("table tbody tr").Count.Should().Be(1);
            _ = cut.Markup.Should().Contain("No matching records found");
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeDeviceListPageShouldShowNoContentWhenProblemDetailsExceptionOccursOnGetModels()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>()
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        new(),
                        new(),
                        new()
                    }
                });

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("table tbody tr").Count.Should().Be(3);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void EdgeDeviceListPageShouldResetOnClickOnReset()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
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

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString()
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

            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
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

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString()
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

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(It.IsAny<string>()))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>
                {
                    Items = Array.Empty<IoTEdgeListItem>()
                });

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString()
                    }
                });

            var cut = RenderComponent<EdgeDeviceListPage>();

            // Act
            cut.WaitForElement("#sortDeviceId").Click();
            cut.WaitForElement("#sortDeviceId").Click();
            cut.WaitForElement("#tableRefreshButton").Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public async Task FliterBySelectModelShould()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        new IoTEdgeListItem()
                        {
                            DeviceId = Guid.NewGuid().ToString(),
                            DeviceName = Guid.NewGuid().ToString(),
                        },
                        new IoTEdgeListItem()
                        {
                            DeviceId = Guid.NewGuid().ToString(),
                            DeviceName = Guid.NewGuid().ToString(),
                        }
                    }
                });

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList())
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = "model_01",
                        Description = Guid.NewGuid().ToString(),
                    },
                    new IoTEdgeModelListItem()
                    {
                        Name = "model_02",
                        Description = Guid.NewGuid().ToString(),
                    },
                });

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();
            var newModelList = await cut.Instance.Search("01");

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            _ = cut.FindAll("table tbody tr").Count.Should().Be(2);
            Assert.AreEqual(1, newModelList.Count());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
