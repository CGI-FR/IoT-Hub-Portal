// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.Settings;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;
    using Portal.Shared.Models.v1._0;

    [TestFixture]
    public class DeviceTagsPageTests : BlazorUnitTest
    {
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IDeviceTagSettingsClientService> mockDeviceTagSettingsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockDeviceTagSettingsClientService = MockRepository.Create<IDeviceTagSettingsClientService>();

            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockDeviceTagSettingsClientService.Object);
        }

        [Test]
        public void DeviceListPageRendersCorrectly()
        {
            // Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(), Name = Guid.NewGuid().ToString(), Required = false,
                        Searchable = false
                    },
                    new()
                    {
                        Label = Guid.NewGuid().ToString(), Name = Guid.NewGuid().ToString(), Required = false,
                        Searchable = false
                    }
                });

            // Act
            var cut = RenderComponent<DeviceTagsPage>();
            cut.WaitForAssertion(() => cut.Find("div.mud-grid"));
            var grid = cut.Find("div.mud-grid");

            // Assert
            Assert.IsNotNull(cut.Markup);
            Assert.AreEqual("Tags", cut.Find(".mud-typography-h6").TextContent);
            Assert.IsNotNull(grid.InnerHtml);
            Assert.AreEqual(4, cut.FindAll("tr").Count);
            Assert.IsNotNull(cut.Find(".mud-table-container"));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnclickOnReloadShouldReloadTags()
        {
            // Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<DeviceTagsPage>();

            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            cut.WaitForAssertion(() => cut.Markup.Should().Contain("No matching records found"));

            MockHttpClient.Clear();

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(Fixture.Build<DeviceTagDto>().CreateMany(3).ToList());

            // Act
            cut.WaitForElement("#reload-tags").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("No matching records found"));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(3));

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceTags()
        {
            // Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceTagsPage>();
            cut.WaitForAssertion(() => cut.Find("div.mud-grid"));
            var grid = cut.Find("div.mud-grid");

            // Assert
            Assert.IsNotNull(cut.Markup);
            Assert.AreEqual("Tags", cut.Find(".mud-typography-h6").TextContent);
            Assert.IsNotNull(grid.InnerHtml);
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));
            Assert.AreEqual(1, cut.FindAll("table tbody tr").Count);
            Assert.IsNotNull(cut.Find(".mud-table-container"));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }


        [Test]
        public void ClickOnSaveShouldCreateUpdateTag()
        {
            //Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = "tagLabel", Name = "tagName", Required = false,
                        Searchable = false
                    }
                });
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.CreateOrUpdateDeviceTag(It.IsAny<DeviceTagDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);


            var cut = RenderComponent<DeviceTagsPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForElement("#saveButton").Click();

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDeleteTag()
        {
            //Arrange
            const string deviceTagName = "tagName";

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new()
                    {
                        Label = "tagLabel", Name = deviceTagName, Required = false,
                        Searchable = false
                    }
                });
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.DeleteDeviceTagByName(deviceTagName))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent<DeviceTagsPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForElement("#deleteButton").Click();

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldDisplayErrorSnackbarIfDuplicated()
        {
            var mockTag = new DeviceTagDto
            {
                Label = "Label",
                Name = "Name",
                Required = false,
                Searchable = false
            };

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    mockTag,
                    mockTag
                });

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Warning, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);
            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);


            var cut = RenderComponent<DeviceTagsPage>();

            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            var saveButton = cut.Find("#saveButton");

            // Act
            saveButton.Click();

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldDisplayErrorSnackbarIfValidationIssue()
        {
            var mockTag = new DeviceTagDto
            {
                Label = "Label",
                Name = "InvalidName!",
                Required = false,
                Searchable = false
            };

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    mockTag,
                    mockTag
                });

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Warning, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);
            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);


            var cut = RenderComponent<DeviceTagsPage>();

            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            var saveButton = cut.Find("#saveButton");

            // Act
            saveButton.Click();

            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldProcessProblemDetailsExceptionIfIssueOccursWhenUpdatingDeviceTags()
        {
            // Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(new List<DeviceTagDto>
                {
                    new () { Label = "Label", Name = "Name", Required = false, Searchable = false }
                });

            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.CreateOrUpdateDeviceTag(It.IsAny<DeviceTagDto>()))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceTagsPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            var saveButton = cut.Find("#saveButton");
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddNewTagShouldAddNewTag()
        {
            //Arrange
            _ = this.mockDeviceTagSettingsClientService.Setup(service => service.GetDeviceTags())
                .ReturnsAsync(Array.Empty<DeviceTagDto>());

            var cut = RenderComponent<DeviceTagsPage>();
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.WaitForElement("#addTagButton").Click();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("No matching records found"));
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
