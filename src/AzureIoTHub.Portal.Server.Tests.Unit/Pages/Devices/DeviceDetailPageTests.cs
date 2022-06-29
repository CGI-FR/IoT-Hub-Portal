// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.Devices;
    using AzureIoTHub.Portal.Client.Shared;
    using Models.v10;
    using Helpers;
    using Bunit;
    using Bunit.TestDoubles;
    using Client.Exceptions;
    using Client.Models;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Mocks;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using RichardSzalay.MockHttp;

    [TestFixture]
    public class DeviceDetailPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private FakeNavigationManager mockNavigationManager;

        private static string ApiBaseUrl => "/api/devices";

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);

            _ = Services.AddSingleton(new PortalSettings { IsLoRaSupported = false });

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            this.mockNavigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {

            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var modelId = Guid.NewGuid().ToString();


            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/devices/{deviceId}")
                .RespondJson(new DeviceDetails() { ModelId = modelId });

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/devices/{deviceId}/properties")
                .RespondJson(new List<DevicePropertyValue>());

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/models/{modelId}")
                .RespondJson(new DeviceModel());

            _ = MockHttpClient
                .When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>());


            // Act
            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", deviceId));
            var returnButton = cut.WaitForElement("#returnButton");

            returnButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().EndWith("/devices"));
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

            _ = MockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceDetails>>(m.Content);
                    var objectContent = m.Content as ObjectContent<DeviceDetails>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<DeviceDetails>(objectContent.Value);
                    var deviceDetails = objectContent.Value as DeviceDetails;
                    Assert.IsNotNull(deviceDetails);

                    Assert.AreEqual(mockDeviceDetails.DeviceID, deviceDetails.DeviceID);
                    Assert.AreEqual(mockDeviceDetails.DeviceName, deviceDetails.DeviceName);
                    Assert.AreEqual(mockDeviceDetails.ModelId, deviceDetails.ModelId);

                    return true;
                })
                .RespondText(string.Empty);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{mockDeviceDetails.DeviceID}")
                .RespondJson(mockDeviceDetails);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceDetails.ModelId}")
                .RespondJson(mockDeviceModel);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    mockTag
                });

            _ = MockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockDeviceDetails.DeviceID));
            cut.WaitForAssertion(() => cut.Find($"#{nameof(DeviceModel.Name)}>b").InnerHtml.Should().NotBeEmpty());

            var saveButton = cut.WaitForElement("#saveButton");
            saveButton.Click();

            // Assert            
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("devices", StringComparison.OrdinalIgnoreCase), 3.Seconds());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void SaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnUpdatingDevice()
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


            _ = MockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceDetails>>(m.Content);
                    var objectContent = m.Content as ObjectContent<DeviceDetails>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<DeviceDetails>(objectContent.Value);
                    var deviceDetails = objectContent.Value as DeviceDetails;
                    Assert.IsNotNull(deviceDetails);

                    Assert.AreEqual(mockDeviceDetails.DeviceID, deviceDetails.DeviceID);
                    Assert.AreEqual(mockDeviceDetails.DeviceName, deviceDetails.DeviceName);
                    Assert.AreEqual(mockDeviceDetails.ModelId, deviceDetails.ModelId);

                    return true;
                })
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{mockDeviceDetails.DeviceID}")
                .RespondJson(mockDeviceDetails);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceDetails.ModelId}")
                .RespondJson(mockDeviceModel);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    mockTag
                });

            _ = MockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockDeviceDetails.DeviceID));

            var saveButton = cut.WaitForElement("#saveButton");
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockNavigationManager.Uri.Should().NotEndWith("devices"));
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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


            _ = MockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceDetails>>(m.Content);
                    var objectContent = m.Content as ObjectContent<DeviceDetails>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<DeviceDetails>(objectContent.Value);
                    var deviceDetails = objectContent.Value as DeviceDetails;
                    Assert.IsNotNull(deviceDetails);

                    Assert.AreEqual(mockDeviceDetails.DeviceID, deviceDetails.DeviceID);
                    Assert.AreEqual(mockDeviceDetails.DeviceName, deviceDetails.DeviceName);
                    Assert.AreEqual(mockDeviceDetails.ModelId, deviceDetails.ModelId);

                    return true;
                })
                .RespondText(string.Empty);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{mockDeviceDetails.DeviceID}")
                .RespondJson(mockDeviceDetails);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceDetails.ModelId}")
                .RespondJson(mockDeviceModel);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    mockTag
                });

            _ = MockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = MockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, null)).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockDeviceDetails.DeviceID));

            cut.WaitForElement($"#{nameof(DeviceDetails.DeviceName)}").Change("");
            var saveButton = cut.WaitForElement("#saveButton");
            saveButton.Click();

            // Assert            
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnConnectShouldDisplayDeviceCredentials()
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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{mockDeviceDetails.DeviceID}")
                .RespondJson(mockDeviceDetails);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceDetails.ModelId}")
                .RespondJson(mockDeviceModel);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    mockTag
                });

            _ = MockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ConnectionStringDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);

            // Act
            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockDeviceDetails.DeviceID));

            var connectButton = cut.WaitForElement("#connectButton");
            connectButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingRequest());
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{mockDeviceDetails.DeviceID}")
                .RespondJson(mockDeviceDetails);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceDetails.ModelId}")
                .RespondJson(mockDeviceModel);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    mockTag
                });

            _ = MockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDevicePage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockDeviceDetails.DeviceID));

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert            
            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndRedirectIfConfirmed()
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

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/devices/{mockDeviceDetails.DeviceID}")
                .RespondJson(mockDeviceDetails);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/models/{mockDeviceDetails.ModelId}")
                .RespondJson(mockDeviceModel);

            _ = MockHttpClient.When(HttpMethod.Get, $"/api/settings/device-tags")
                .RespondJson(new List<DeviceTag>()
                {
                    mockTag
                });

            _ = MockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/{mockDeviceDetails.DeviceID}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDevicePage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceDetailPage>(ComponentParameter.CreateParameter("DeviceID", mockDeviceDetails.DeviceID));

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert            
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/devices", StringComparison.OrdinalIgnoreCase));

            cut.WaitForAssertion(() => MockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
