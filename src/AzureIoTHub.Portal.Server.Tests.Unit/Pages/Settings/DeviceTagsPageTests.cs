// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.Settings;
    using AzureIoTHub.Portal.Client.Shared;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
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
    public class DeviceTagsPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;

        private static string ApiBaseUrl => "/api/settings/device-tags";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = this.testContext.Services.AddMockHttpClient();

            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);

            this.mockSnackbarService = this.mockRepository.Create<ISnackbar>();
            _ = this.testContext.Services.AddSingleton(this.mockSnackbarService.Object);

            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudElementRef.restoreFocus", _ => true);

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void DeviceListPageRendersCorrectly()
        {
            // Arrange
            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new List<DeviceTag>(){
                    new DeviceTag
                        { Label =  Guid.NewGuid().ToString(), Name = Guid.NewGuid().ToString(), Required = false, Searchable = false },
                    new DeviceTag
                        { Label = Guid.NewGuid().ToString(), Name = Guid.NewGuid().ToString(), Required = false, Searchable = false }
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

            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceTags()
        {
            // Arrange
            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

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
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }


        [Test]
        public void ClickOnSaveShouldUpdateTagList()
        {
            var mockTag = new DeviceTag
            {
                Label = "Label",
                Name = "Name",
                Required = false,
                Searchable = false
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new List<DeviceTag>(){
                    mockTag
                });

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceTag>>>(m.Content);
                    var objectContent = m.Content as ObjectContent<List<DeviceTag>>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<List<DeviceTag>>(objectContent.Value);
                    var tags = objectContent.Value as IEnumerable<DeviceTag>;
                    Assert.IsNotNull(tags);

                    Assert.AreEqual(1, tags.Count());

                    var tag = tags.Single(x => x.Name == mockTag.Name);

                    Assert.AreEqual(mockTag.Name, tag.Name);
                    Assert.AreEqual(mockTag.Label, tag.Label);
                    Assert.AreEqual(mockTag.Required, tag.Required);
                    Assert.AreEqual(mockTag.Searchable, tag.Searchable);

                    return true;
                })
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);


            var cut = RenderComponent<DeviceTagsPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            cut.WaitForAssertion(() => cut.Markup.Should().NotContain("Loading..."));

            // Act
            cut.Find("#saveButton").Click();

            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldDisplayErrorSnackbarIfDuplicated()
        {
            var mockTag = new DeviceTag
            {
                Label = "Label",
                Name = "Name",
                Required = false,
                Searchable = false
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new List<DeviceTag>(){
                    mockTag,
                    mockTag
                });

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceTag>>>(m.Content);
                    var objectContent = m.Content as ObjectContent<List<DeviceTag>>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<List<DeviceTag>>(objectContent.Value);
                    var tags = objectContent.Value as IEnumerable<DeviceTag>;
                    Assert.IsNotNull(tags);

                    Assert.AreEqual(1, tags.Count());

                    var tag = tags.Single(x => x.Name == mockTag.Name);

                    Assert.AreEqual(mockTag.Name, tag.Name);
                    Assert.AreEqual(mockTag.Label, tag.Label);
                    Assert.AreEqual(mockTag.Required, tag.Required);
                    Assert.AreEqual(mockTag.Searchable, tag.Searchable);

                    return true;
                })
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Warning, null)).Returns((Snackbar)null);
            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, null)).Returns((Snackbar)null);


            var cut = RenderComponent<DeviceTagsPage>();

            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            var saveButton = cut.Find("#saveButton");

            // Act
            saveButton.Click();

            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldDisplayErrorSnackbarIfValidationIssue()
        {
            var mockTag = new DeviceTag
            {
                Label = "Label",
                Name = "InvalidName!",
                Required = false,
                Searchable = false
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new List<DeviceTag>(){
                    mockTag,
                    mockTag
                });

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceTag>>>(m.Content);
                    var objectContent = m.Content as ObjectContent<List<DeviceTag>>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<List<DeviceTag>>(objectContent.Value);
                    var tags = objectContent.Value as IEnumerable<DeviceTag>;
                    Assert.IsNotNull(tags);

                    Assert.AreEqual(1, tags.Count());

                    var tag = tags.Single(x => x.Name == mockTag.Name);

                    Assert.AreEqual(mockTag.Name, tag.Name);
                    Assert.AreEqual(mockTag.Label, tag.Label);
                    Assert.AreEqual(mockTag.Required, tag.Required);
                    Assert.AreEqual(mockTag.Searchable, tag.Searchable);

                    return true;
                })
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Warning, null)).Returns((Snackbar)null);
            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, null)).Returns((Snackbar)null);


            var cut = RenderComponent<DeviceTagsPage>();

            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            var saveButton = cut.Find("#saveButton");

            // Act
            saveButton.Click();

            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldProcessProblemDetailsExceptionIfIssueOccursWhenUpdatingDeviceTags()
        {
            // Arrange
            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new List<DeviceTag>(){
                    new DeviceTag
                        { Label = "Label", Name = "Name", Required = false, Searchable = false }
                    });

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            var cut = RenderComponent<DeviceTagsPage>();
            cut.WaitForAssertion(() => cut.Find("#saveButton"));
            var saveButton = cut.Find("#saveButton");
            saveButton.Click();

            // Assert            
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
