// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DevicesModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
    using AzureIoTHub.Portal.Models;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
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
    using AzureIoTHub.Portal.Client.Shared;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;

    [TestFixture]
    public class DeviceModelDetaislPageTests : IDisposable
    {
        private Bunit.TestContext testContext;
        private MockHttpMessageHandler mockHttpClient;

        private readonly string mockModelId = Guid.NewGuid().ToString();

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private FakeNavigationManager mockNavigationManager;

        private string ApiBaseUrl => $"/api/models/{this.mockModelId}";
        private string LorawanApiBaseUrl => $"/api/lorawan/models/{this.mockModelId}";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockHttpClient = this.testContext.Services
                                            .AddMockHttpClient();

            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            _ = this.testContext.Services.AddSingleton(this.mockDialogService.Object);

            this.mockSnackbarService = this.mockRepository.Create<ISnackbar>();
            _ = this.testContext.Services.AddSingleton(this.mockSnackbarService.Object);

            _ = this.testContext.Services.AddMudServices();

            _ = this.testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            _ = this.testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            _ = this.testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);
            _ = this.testContext.JSInterop.SetupVoid("mudElementRef.restoreFocus", _ => true);

            this.mockNavigationManager = this.testContext.Services.GetRequiredService<FakeNavigationManager>();

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void ClickOnSaveShouldPostDeviceModelData()
        {
            // Arrange
            var expectedProperties = Enumerable.Range(0, 2)
                .Select(_ => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Double
                }).ToArray();

            var expectedModel = SetupMockDeviceModel(properties: expectedProperties);

            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceModel>>(m.Content);
                    var objectContent = m.Content as ObjectContent<DeviceModel>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<DeviceModel>(objectContent.Value);
                    var deviceModel = objectContent.Value as DeviceModel;
                    Assert.IsNotNull(deviceModel);

                    Assert.AreEqual(expectedModel.ModelId, deviceModel.ModelId);
                    Assert.AreEqual(expectedModel.Name, deviceModel.Name);
                    Assert.AreEqual(expectedModel.Description, deviceModel.Description);
                    Assert.AreEqual(expectedModel.SupportLoRaFeatures, false);
                    Assert.AreEqual(expectedModel.IsBuiltin, false);

                    return true;
                })
                .RespondText(string.Empty);

            _ = this.mockHttpClient
                .When(HttpMethod.Post, $"{ApiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var objectContent = m.Content as ObjectContent<List<DeviceProperty>>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<List<DeviceProperty>>(objectContent.Value);
                    var properties = objectContent.Value as IEnumerable<DeviceProperty>;
                    Assert.IsNotNull(properties);

                    Assert.AreEqual(expectedProperties.Length, properties?.Count());

                    foreach (var expectedProperty in expectedProperties)
                    {
                        var property = properties?.Single(x => x.Name == expectedProperty.Name);

                        Assert.AreEqual(expectedProperty.Name, property.Name);
                        Assert.AreEqual(expectedProperty.DisplayName, property.DisplayName);
                        Assert.AreEqual(expectedProperty.PropertyType, property.PropertyType);
                    }

                    return true;
                })
                .RespondText(string.Empty);


            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));
            var saveButton = cut.WaitForElement("#saveButton");

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldProcessProblemDetailsExceptionIfIssueOccursWhenUpdatingDeviceModel()
        {
            // Arrange
            var expectedProperties = Enumerable.Range(0, 2)
                .Select(_ => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Double
                }).ToArray();

            var expectedModel = SetupMockDeviceModel(properties: expectedProperties);

            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceModel>>(m.Content);
                    var objectContent = m.Content as ObjectContent<DeviceModel>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<DeviceModel>(objectContent.Value);
                    var deviceModel = objectContent.Value as DeviceModel;
                    Assert.IsNotNull(deviceModel);

                    Assert.AreEqual(expectedModel.ModelId, deviceModel.ModelId);
                    Assert.AreEqual(expectedModel.Name, deviceModel.Name);
                    Assert.AreEqual(expectedModel.Description, deviceModel.Description);
                    Assert.AreEqual(expectedModel.SupportLoRaFeatures, false);
                    Assert.AreEqual(expectedModel.IsBuiltin, false);

                    return true;
                })
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockHttpClient
                .When(HttpMethod.Post, $"{ApiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var objectContent = m.Content as ObjectContent<List<DeviceProperty>>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<List<DeviceProperty>>(objectContent.Value);
                    var properties = objectContent.Value as IEnumerable<DeviceProperty>;
                    Assert.IsNotNull(properties);

                    Assert.AreEqual(expectedProperties.Length, properties?.Count());

                    foreach (var expectedProperty in expectedProperties)
                    {
                        var property = properties?.Single(x => x.Name == expectedProperty.Name);

                        Assert.AreEqual(expectedProperty.Name, property.Name);
                        Assert.AreEqual(expectedProperty.DisplayName, property.DisplayName);
                        Assert.AreEqual(expectedProperty.PropertyType, property.PropertyType);
                    }

                    return true;
                })
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));


            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var saveButton = cut.WaitForElement("#saveButton");
            saveButton.Click();

            cut.WaitForState(() => !this.mockNavigationManager.Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldDisplaySnackbarIfValidationError()
        {
            // Arrange
            var expectedProperties = Enumerable.Range(0, 2)
                .Select(_ => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Double
                }).ToArray();

            var expectedModel = SetupMockDeviceModel(properties: expectedProperties);

            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<DeviceModel>>(m.Content);
                    var objectContent = m.Content as ObjectContent<DeviceModel>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<DeviceModel>(objectContent.Value);
                    var deviceModel = objectContent.Value as DeviceModel;
                    Assert.IsNotNull(deviceModel);

                    Assert.AreEqual(expectedModel.ModelId, deviceModel.ModelId);
                    Assert.AreEqual(expectedModel.Name, deviceModel.Name);
                    Assert.AreEqual(expectedModel.Description, deviceModel.Description);
                    Assert.AreEqual(expectedModel.SupportLoRaFeatures, false);
                    Assert.AreEqual(expectedModel.IsBuiltin, false);

                    return true;
                })
                .RespondText(string.Empty);

            _ = this.mockHttpClient
                .When(HttpMethod.Post, $"{ApiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var objectContent = m.Content as ObjectContent<List<DeviceProperty>>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<List<DeviceProperty>>(objectContent.Value);
                    var properties = objectContent.Value as IEnumerable<DeviceProperty>;
                    Assert.IsNotNull(properties);

                    Assert.AreEqual(expectedProperties.Length, properties?.Count());

                    foreach (var expectedProperty in expectedProperties)
                    {
                        var property = properties?.Single(x => x.Name == expectedProperty.Name);

                        Assert.AreEqual(expectedProperty.Name, property.Name);
                        Assert.AreEqual(expectedProperty.DisplayName, property.DisplayName);
                        Assert.AreEqual(expectedProperty.PropertyType, property.PropertyType);
                    }

                    return true;
                })
                .RespondText(string.Empty);

            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, null)).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            cut.WaitForElement($"#{nameof(DeviceModel.Name)}").Change("");
            var saveButton = cut.WaitForElement("#saveButton");
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddPropertyShouldAddNewProperty()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new DeviceModel
                {
                    ModelId = this.mockModelId,
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/avatar")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/properties")
                .RespondJson(Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var objectContent = m.Content as ObjectContent<List<DeviceProperty>>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<List<DeviceProperty>>(objectContent.Value);
                    var properties = objectContent.Value as IEnumerable<DeviceProperty>;
                    Assert.IsNotNull(properties);

                    Assert.AreEqual(1, properties.Count());

                    var property = properties.Single(x => x.Name == propertyName);

                    Assert.AreEqual(propertyName, property.Name);
                    Assert.AreEqual(displayName, property.DisplayName);
                    Assert.AreEqual(DevicePropertyType.Boolean, property.PropertyType);
                    Assert.IsTrue(property.IsWritable);

                    return true;
                })
                .RespondText(string.Empty);


            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var saveButton = cut.WaitForElement("#saveButton");
            var addPropertyButton = cut.WaitForElement("#addPropertyButton");
            addPropertyButton.Click();

            cut.WaitForElement($"#property- #{nameof(DeviceProperty.Name)}").Change(propertyName);

            var propertyCssSelector = $"#property-{propertyName}";

            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.DisplayName)}").Change(displayName);
            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.PropertyType)}").Change(nameof(DevicePropertyType.Boolean));
            cut.WaitForElement($"{propertyCssSelector} #{nameof(DeviceProperty.IsWritable)}").Change(true);

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnRemovePropertyShouldRemoveTheProperty()
        {
            // Arrange
            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new DeviceModel
                {
                    ModelId = this.mockModelId,
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/avatar")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/properties")
                .RespondJson(new DeviceProperty[]
                {
                    new DeviceProperty()
                });

            _ = this.mockHttpClient.When(HttpMethod.Put, $"{ApiBaseUrl}")
                .RespondText(string.Empty);

            _ = this.mockHttpClient.When(HttpMethod.Post, $"{ApiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<ObjectContent<List<DeviceProperty>>>(m.Content);
                    var objectContent = m.Content as ObjectContent<List<DeviceProperty>>;
                    Assert.IsNotNull(objectContent);

                    Assert.IsAssignableFrom<List<DeviceProperty>>(objectContent.Value);
                    var properties = objectContent.Value as IEnumerable<DeviceProperty>;
                    Assert.IsNotNull(properties);

                    Assert.AreEqual(0, properties.Count());

                    return true;
                })
                .RespondText(string.Empty);


            var mockDialogReference = new DialogReference(Guid.NewGuid(), this.mockDialogService.Object);
            _ = this.mockDialogService.Setup(c => c.Show<ProcessingDialog>("Processing", It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference);
            _ = this.mockDialogService.Setup(c => c.Close(It.Is<DialogReference>(x => x == mockDialogReference)));

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, null)).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var saveButton = cut.WaitForElement("#saveButton");
            var removePropertyButton = cut.WaitForElement("#DeletePropertyButton");
            removePropertyButton.Click();

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void WhenPresentModelDetailsShouldDisplayProperties()
        {
            // Arrange
            var properties = Enumerable.Range(0, 10)
                .Select(_ => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = DevicePropertyType.Double
                }).ToArray();

            _ = SetupMockDeviceModel(properties: properties);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            cut.WaitForAssertion(() => cut.Find("#form"));

            foreach (var item in properties)
            {
                var propertyCssSelector = $"#property-{item.Name}";

                _ = cut.WaitForElement(propertyCssSelector);
                Assert.AreEqual(item.DisplayName, cut.Find($"{propertyCssSelector} #{nameof(item.DisplayName)}").Attributes["value"].Value);
                Assert.AreEqual(item.Name, cut.Find($"{propertyCssSelector} #{nameof(item.Name)}").Attributes["value"].Value);
                Assert.AreEqual(item.PropertyType.ToString(), cut.Find($"{propertyCssSelector} #{nameof(item.PropertyType)}").Attributes["value"].Value);
                Assert.AreEqual(item.IsWritable.ToString().ToLowerInvariant(), cut.Find($"{propertyCssSelector} #{nameof(item.IsWritable)}").Attributes["aria-checked"].Value);
            }

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureIsDisabledModelDetailsShouldNotDisplayLoRaWANTab()
        {
            // Arrange
            _ = SetupMockDeviceModel();


            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            cut.WaitForAssertion(() => cut.Find("#form"));

            var tabs = cut.WaitForElements(".mud-tabs .mud-tab");
            Assert.AreEqual(1, tabs.Count);
            Assert.AreEqual("General", tabs.Single().TextContent);

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureIsEnabledModelDetailsShouldDisplayLoRaWANTab()
        {
            // Arrange
            _ = SetupMockLoRaWANDeviceModel();

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>(
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId),
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.IsLoRa), true));

            cut.WaitForAssertion(() => cut.Find("#form"));

            var tabs = cut.WaitForElements(".mud-tabs .mud-tab");
            Assert.AreEqual(2, tabs.Count);
            Assert.AreEqual("General", tabs[0].TextContent);
            Assert.AreEqual("LoRaWAN", tabs[1].TextContent);

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModel()
        {
            // Arrange
            _ = SetupMockLoRaWANDeviceModelThrowingException();

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>(
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId),
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.IsLoRa), true));

            var loading = cut.WaitForElements(".mud-progress-circular-svg");
            Assert.IsNotEmpty(loading);

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        private DeviceModel SetupMockDeviceModel(DeviceProperty[] properties = null)
        {
            var deviceModel = new DeviceModel
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = new Uri($"http://fake.local/{this.mockModelId}"),
                SupportLoRaFeatures = false
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, ApiBaseUrl)
                                .RespondJson(deviceModel);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/avatar")
                    .RespondText($"http://fake.local/{this.mockModelId}");

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{ApiBaseUrl}/properties")
                .RespondJson(properties ?? Array.Empty<DeviceProperty>());

            return deviceModel;
        }

        private LoRaDeviceModel SetupMockLoRaWANDeviceModel(DeviceProperty[] properties = null, DeviceModelCommand[] commands = null)
        {
            var deviceModel = new LoRaDeviceModel
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = new Uri($"http://fake.local/{this.mockModelId}"),
                SupportLoRaFeatures = true
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, LorawanApiBaseUrl)
                    .RespondJson(deviceModel);

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/avatar")
                    .RespondText($"http://fake.local/{this.mockModelId}");

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/commands")
                .RespondJson(commands ?? Array.Empty<DeviceModelCommand>());

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/properties")
                    .RespondJson(properties ?? Array.Empty<DeviceProperty>());

            _ = this.mockHttpClient.Fallback
                .With(_ =>
                {
                    Debugger.Break();
                    return true;
                });

            return deviceModel;
        }

        private LoRaDeviceModel SetupMockLoRaWANDeviceModelThrowingException()
        {
            var deviceModel = new LoRaDeviceModel
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = new Uri($"http://fake.local/{this.mockModelId}"),
                SupportLoRaFeatures = true
            };

            _ = this.mockHttpClient.When(HttpMethod.Get, LorawanApiBaseUrl)
                    .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/avatar")
                    .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/commands")
                .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockHttpClient.When(HttpMethod.Get, $"{LorawanApiBaseUrl}/properties")
                    .Throw(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            _ = this.mockHttpClient.Fallback
                .With(_ =>
                {
                    Debugger.Break();
                    return true;
                });

            return deviceModel;
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {

            // Arrange
            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{ApiBaseUrl}/properties")
                .RespondJson(new List<DeviceProperty>());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{ApiBaseUrl}")
                .RespondJson(new DeviceModel());

            _ = this.mockHttpClient
                .When(HttpMethod.Get, $"{ApiBaseUrl}/avatar")
                .RespondText(string.Empty);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>(ComponentParameter.CreateParameter("ModelId", this.mockModelId ));
            var returnButton = cut.WaitForElement("#returnButton");

            returnButton.Click();
            cut.WaitForState(() => this.testContext.Services.GetRequiredService<FakeNavigationManager>().Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            // Arrange
            _ = SetupMockDeviceModel();

            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var mockDialogReference = this.mockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
            cut.WaitForAssertion(() => this.mockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndRedirectIfConfirmed()
        {
            // Arrange
            _ = SetupMockDeviceModel();

            var mockDialogReference = this.mockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models", StringComparison.OrdinalIgnoreCase));

            // Assert
            cut.WaitForAssertion(() => this.mockHttpClient.VerifyNoOutstandingExpectation());
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
