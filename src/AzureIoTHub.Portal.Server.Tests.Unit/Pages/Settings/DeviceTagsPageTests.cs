// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using AzureIoTHub.Portal.Client.Pages.Settings;
    using AzureIoTHub.Portal.Client.Shared;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
    using Bunit.TestDoubles;
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
        private FakeNavigationManager mockNavigationManager;

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

            this.mockNavigationManager = this.testContext.Services.GetRequiredService<FakeNavigationManager>();

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
            where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
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
                    var jsonContent = m.Content as ObjectContent<List<DeviceTag>>;

                    Assert.IsAssignableFrom<List<DeviceTag>>(jsonContent.Value);
                    var tags = jsonContent.Value as IEnumerable<DeviceTag>;

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

            var saveButton = cut.WaitForElement("#saveButton");

            // Act
            saveButton.Click();
            Thread.Sleep(1000);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
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
                    var jsonContent = m.Content as ObjectContent<List<DeviceTag>>;

                    Assert.IsAssignableFrom<List<DeviceTag>>(jsonContent.Value);
                    var tags = jsonContent.Value as IEnumerable<DeviceTag>;

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

            var saveButton = cut.WaitForElement("#saveButton");

            // Act
            saveButton.Click();
            Thread.Sleep(1000);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
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
                    var jsonContent = m.Content as ObjectContent<List<DeviceTag>>;

                    Assert.IsAssignableFrom<List<DeviceTag>>(jsonContent.Value);
                    var tags = jsonContent.Value as IEnumerable<DeviceTag>;

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

            var saveButton = cut.WaitForElement("#saveButton");

            // Act
            saveButton.Click();
            Thread.Sleep(1000);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
            this.mockRepository.VerifyAll();
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
