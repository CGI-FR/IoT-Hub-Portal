// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.EdgeDevices
{
    using System;
    using System.Collections.Generic;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.EdgeDevices;
    using IoTHub.Portal.Client.Dialogs.EdgeDevices;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using System.Linq;
    using MudBlazor;
    using AutoFixture;
    using Portal.Shared;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class EdgeDeviceListPageTests : BlazorUnitTest
    {
        private Mock<IEdgeDeviceClientService> mockEdgeDeviceClientService;
        private Mock<IEdgeModelClientService> mockEdgeModelClientService;
        private Mock<IDialogService> mockDialogService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockEdgeDeviceClientService = MockRepository.Create<IEdgeDeviceClientService>();
            this.mockEdgeModelClientService = MockRepository.Create<IEdgeModelClientService>();

            _ = Services.AddSingleton(this.mockEdgeDeviceClientService.Object);
            _ = Services.AddSingleton(this.mockEdgeModelClientService.Object);
            _ = Services.AddSingleton(this.mockDialogService.Object);
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

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Fixture.CreateMany<LabelDto>(5).ToList());

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
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

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString()
                    }
                });

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

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

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString()
                    }
                });

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

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

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
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

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
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

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
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

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
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
            var modelList = new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        ModelId = Guid.NewGuid().ToString(),
                        Name = "model_01",
                        Description = Guid.NewGuid().ToString(),
                    },
                    new IoTEdgeModelListItem()
                    {
                        ModelId = Guid.NewGuid().ToString(),
                        Name = "model_02",
                        Description = Guid.NewGuid().ToString(),
                    },
                };
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
            var expectedUrlFilter = $"api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId={modelList[0].ModelId}";
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

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrlFilter))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>
                {
                    Items = new List<IoTEdgeListItem>
                    {
                        new IoTEdgeListItem()
                        {
                            DeviceId = Guid.NewGuid().ToString(),
                            DeviceName = Guid.NewGuid().ToString(),
                        }
                    }
                });

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
                .ReturnsAsync(modelList);

            // Act
            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EdgeDeviceListPage>();

            cut.WaitForElement($"#{nameof(IoTEdgeModelListItem.ModelId)}").Click();

            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll(".mud-input-helper-text").Count.Should().Be(2));

            var newModelList = await cut.Instance.Search("01");

            await cut.Instance.ChangeModel(modelList[0]);

            cut.WaitForElement("#searchFilterButton").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            Assert.AreEqual(1, newModelList.Count());
            _ = cut.FindAll("table tbody tr").Count.Should().Be(1);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldShowDeleteDialog()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";
            var deviceList = new List<IoTEdgeListItem>
                    {
                        Fixture.Create<IoTEdgeListItem>()
                    };

            _ = this.mockEdgeDeviceClientService.Setup(service => service.GetDevices(expectedUrl))
                .ReturnsAsync(new PaginationResult<IoTEdgeListItem>
                {
                    Items = deviceList
                });

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
                .ReturnsAsync(new List<IoTEdgeModelListItem>()
                {
                    new IoTEdgeModelListItem()
                    {
                        Name = Guid.NewGuid().ToString(),
                        Description = Guid.NewGuid().ToString(),
                    }
                });

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<EdgeDeviceDeleteConfirmationDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            // Act
            var cut = RenderComponent<EdgeDeviceListPage>();

            var deleteIcon = cut.WaitForElement($"#delete_{deviceList[0].DeviceId}");
            deleteIcon.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SearchEdgeDeviceModels_InputExisingEdgeDeviceModelName_EdgeDeviceModelReturned()
        {
            // Arrange
            var expectedUrl = "api/edge/devices?pageNumber=0&pageSize=10&searchText=&searchStatus=&orderBy=&modelId=";

            var edgeDeviceModels = Fixture.CreateMany<IoTEdgeModelListItem>(2).ToList();
            var expectedEdgeDeviceModel = edgeDeviceModels.First();

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

            _ = this.mockEdgeDeviceClientService.Setup(x => x.GetAvailableLabels())
                .ReturnsAsync(Array.Empty<LabelDto>());

            _ = this.mockEdgeModelClientService.Setup(service => service.GetIoTEdgeModelList(null))
                .ReturnsAsync(edgeDeviceModels);

            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EdgeDeviceListPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            var autocompleteComponent = cut.FindComponent<MudAutocomplete<IoTEdgeModelListItem>>();

            // Act
            autocompleteComponent.Find("input").Input(expectedEdgeDeviceModel.Name);

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-popover-open").Count.Should().Be(1));
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));

            var items = popoverProvider.FindComponents<MudListItem>().ToArray();
            _ = items.Length.Should().Be(1);
            _ = items.First().Markup.Should().Contain(expectedEdgeDeviceModel.Name);

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
