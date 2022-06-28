// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using Models.v10;
    using Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using Client.Shared;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Mocks;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class CreateDevicePageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;

        private static string ApiBaseUrl => "/api/devices";

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
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


            _ = MockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models")
                .RespondJson(new[]
                {
                    mockDeviceModel
                });

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceModel.ModelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/{expectedDeviceDetails.DeviceID}/properties")
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
            Thread.Sleep(3000);

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/devices"));
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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models")
                .RespondJson(new[]
                {
                    mockDeviceModel
                });

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDevicePage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModels()
        {
            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDevicePage>();

            // Assert
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
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


            _ = MockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}")
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

            _ = MockHttpClient.When(HttpMethod.Get, "/api/models")
                .RespondJson(new[]
                {
                    mockDeviceModel
                });

            _ = MockHttpClient.When(HttpMethod.Get, "/api/settings/device-tags")
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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceModel.ModelId}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/{expectedDeviceDetails.DeviceID}/properties")
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
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().NotEndWith("devices"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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

            _ = MockHttpClient.When(HttpMethod.Get, "/api/models")
                .RespondJson(new[]
                {
                    mockDeviceModel
                });

            _ = MockHttpClient.When(HttpMethod.Get, "/api/settings/device-tags")
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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceModel.ModelId}/properties")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            var cut = RenderComponent<CreateDevicePage>();

            // Act
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change(expectedDeviceDetails.DeviceName);
            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceID)}").Change(expectedDeviceDetails.DeviceID);
            await cut.Instance.ChangeModel(mockDeviceModel);

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
        }
    }
}
