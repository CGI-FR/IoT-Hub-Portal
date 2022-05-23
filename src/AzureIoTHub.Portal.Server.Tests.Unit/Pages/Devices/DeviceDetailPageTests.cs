// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using AzureIoTHub.Portal.Client.Pages.Devices;
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
    public class DeviceDetailPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;
        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private FakeNavigationManager mockNavigationManager;

        private static string ApiBaseUrl => "/api/devices";

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
        public void ReturnButtonMustNavigateToPreviousPage()
        {

            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();


            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/devices/{deviceId}")
                .RespondJson(new DeviceDetails() { ModelId = modelId });

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/devices/{deviceId}/properties")
                .RespondJson(new List<DevicePropertyValue>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>());

            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", deviceId));
            var returnButton = cut.WaitForElement("#returnButton");

            // Act
            returnButton.Click();

            // Assert
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/devices", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void ClickOnSaveShouldPutDeviceDetails()
        {
            var mockDeviceModel = new DeviceModel
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var mockTag = new DeviceTag
            {
                Label = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Required = false,
                Searchable = false
            };

            var mockDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
                Tags = new Dictionary<string, string>()
                {
                    {mockTag.Name,Guid.NewGuid().ToString()}
                }
            };


            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<DeviceDetails>(jsonContent.Value);
                    var deviceDetails = jsonContent.Value as DeviceDetails;

                    Assert.AreEqual(mockDeviceDetails.DeviceID, deviceDetails.DeviceID);
                    Assert.AreEqual(mockDeviceDetails.DeviceName, deviceDetails.DeviceName);
                    Assert.AreEqual(mockDeviceDetails.ModelId, deviceDetails.ModelId);

                    return true;
                })
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/devices/{mockDeviceDetails.DeviceID}")
                .RespondJson(mockDeviceDetails);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceDetails.ModelId}")
                .RespondJson(mockDeviceModel);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    mockTag
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondText(string.Empty);

            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockDeviceDetails.DeviceID));
            Thread.Sleep(2500);

            var saveButton = cut.WaitForElement("#returnButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            saveButton.Click();
            Thread.Sleep(2500);
            cut.WaitForState(() =>
            {
                return this.mockNavigationManager.Uri.EndsWith("/devices", StringComparison.OrdinalIgnoreCase);
            });

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void ClickOnSaveShouldDisplaySnackbarIfValidationError()
        {
            var mockDeviceModel = new DeviceModel
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var mockTag = new DeviceTag
            {
                Label = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Required = false,
                Searchable = false
            };

            var mockDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
                Tags = new Dictionary<string, string>()
                {
                    {mockTag.Name,Guid.NewGuid().ToString()}
                }
            };


            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<DeviceDetails>(jsonContent.Value);
                    var deviceDetails = jsonContent.Value as DeviceDetails;

                    Assert.AreEqual(mockDeviceDetails.DeviceID, deviceDetails.DeviceID);
                    Assert.AreEqual(mockDeviceDetails.DeviceName, deviceDetails.DeviceName);
                    Assert.AreEqual(mockDeviceDetails.ModelId, deviceDetails.ModelId);

                    return true;
                })
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/devices/{mockDeviceDetails.DeviceID}")
                .RespondJson(mockDeviceDetails);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceDetails.ModelId}")
                .RespondJson(mockDeviceModel);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    mockTag
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondText(string.Empty);

            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockDeviceDetails.DeviceID));
            Thread.Sleep(2500);

            cut.Find($"#{nameof(DeviceDetails.DeviceName)}").Change("");
            var saveButton = cut.WaitForElement("#saveButton");

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);

            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add("One or more validation errors occurred", Severity.Error, null));

            // Act
            saveButton.Click();
            Thread.Sleep(2500);

            // Assert            
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
