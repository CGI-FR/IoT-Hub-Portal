// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Client.Shared;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Interop;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateDevicePageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;

        private FakeNavigationManager mockNavigationManager;

        private static string ApiBaseUrl => "/api/devices";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            this.mockHttpClient = this.testContext.Services
                                            .AddMockHttpClient();

            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);

            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
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
        public async Task ClickOnSaveShouldPostDeviceDetailsAsync()
        {
            var mockDeviceModel = new DeviceModel
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var expectedDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
            };


            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceDetails>>(m.Content);
                    var objectContent = m.Content as ObjectContent<DeviceDetails>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<DeviceDetails>(objectContent.Value);
                    var deviceDetails = objectContent.Value as DeviceDetails;
                    Assert.IsNotNull(deviceDetails);

                    Assert.AreEqual(expectedDeviceDetails.DeviceID, deviceDetails.DeviceID);
                    Assert.AreEqual(expectedDeviceDetails.DeviceName, deviceDetails.DeviceName);
                    Assert.AreEqual(expectedDeviceDetails.ModelId, deviceDetails.ModelId);

                    return true;
                })
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/models")
                .RespondJson(new DeviceModel[]
                {
                    mockDeviceModel
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    new DeviceTag()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceModel.ModelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/{expectedDeviceDetails.DeviceID}/properties")
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            var cut = RenderComponent<CreateDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            // Act
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            saveButton.Click();

            // Assert            
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/devices", StringComparison.OrdinalIgnoreCase), 3.Seconds());
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceTags()
        {
            var mockDeviceModel = new DeviceModel
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/models")
                .RespondJson(new DeviceModel[]
                {
                    mockDeviceModel
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDevicePage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/models")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDevicePage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public async Task SaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnCreatingDevice()
        {
            var mockDeviceModel = new DeviceModel
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var expectedDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
            };


            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceDetails>>(m.Content);
                    var objectContent = m.Content as ObjectContent<DeviceDetails>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<DeviceDetails>(objectContent.Value);
                    var deviceDetails = objectContent.Value as DeviceDetails;
                    Assert.IsNotNull(deviceDetails);

                    Assert.AreEqual(expectedDeviceDetails.DeviceID, deviceDetails.DeviceID);
                    Assert.AreEqual(expectedDeviceDetails.DeviceName, deviceDetails.DeviceName);
                    Assert.AreEqual(expectedDeviceDetails.ModelId, deviceDetails.ModelId);

                    return true;
                })
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockHttpClient.When(HttpMethod.Get, "/api/models")
                .RespondJson(new DeviceModel[]
                {
                    mockDeviceModel
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceModel.ModelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/{expectedDeviceDetails.DeviceID}/properties")
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            var cut = RenderComponent<CreateDevicePage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
            cut.WaitForState(() => !this.mockNavigationManager.Uri.EndsWith("devices", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task ChangeModelShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingModelProperties()
        {
            var mockDeviceModel = new DeviceModel
            {
                ModelId = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                SupportLoRaFeatures = false,
                Name = Guid.NewGuid().ToString()
            };

            var expectedDeviceDetails = new DeviceDetails
            {
                DeviceName = Guid.NewGuid().ToString(),
                ModelId = mockDeviceModel.ModelId,
                DeviceID = Guid.NewGuid().ToString(),
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, "/api/models")
                .RespondJson(new DeviceModel[]
                {
                    mockDeviceModel
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, "/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>
                {
                    new()
                    {
                        Label = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString(),
                        Required = false,
                        Searchable = false
                    }
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceModel.ModelId}/properties")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<CreateDevicePage>();

            // Act
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
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
