// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.DevicesModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Pages.DeviceModels;
    using IoTHub.Portal.Client.Services;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using Portal.Shared.Models;
    using Portal.Shared.Models.v1._0;
    using Portal.Shared.Models.v1._0.LoRaWAN;
    using UnitTests.Mocks;

    [TestFixture]
    public class DeviceModelDetaislPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;

        private readonly string mockModelId = Guid.NewGuid().ToString();

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));
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

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            var expectedModel = SetupMockDeviceModel(properties: expectedProperties);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.UpdateDeviceModel(It.Is<DeviceModelDto>(model =>
                        expectedModel.ModelId.Equals(model.ModelId, StringComparison.Ordinal) && expectedModel.Name.Equals(model.Name, StringComparison.Ordinal) && expectedModel.Description.Equals(model.Description, StringComparison.Ordinal) && !model.SupportLoRaFeatures && !model.IsBuiltin)))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.SetDeviceModelModelProperties(It.IsAny<string>(), It.Is<List<DeviceProperty>>(list => list.Count.Equals(expectedProperties.Length))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));
            var saveButton = cut.WaitForElement("#saveButton");

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            var expectedModel = SetupMockDeviceModel(properties: expectedProperties);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                        service.UpdateDeviceModel(It.Is<DeviceModelDto>(model =>
                            expectedModel.ModelId.Equals(model.ModelId, StringComparison.Ordinal) && expectedModel.Name.Equals(model.Name, StringComparison.Ordinal) && expectedModel.Description.Equals(model.Description, StringComparison.Ordinal) && !model.SupportLoRaFeatures && !model.IsBuiltin)))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var saveButton = cut.WaitForElement("#saveButton");
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().NotEndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
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

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });


            _ = SetupMockDeviceModel(properties: expectedProperties);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            cut.WaitForElement($"#{nameof(DeviceModelDto.Name)}").Change("");
            var saveButton = cut.WaitForElement("#saveButton");
            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().NotEndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddPropertyShouldAddNewProperty()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(this.mockModelId))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = this.mockModelId,
                    Name = Guid.NewGuid().ToString()
                });

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });


            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(this.mockModelId))
                .ReturnsAsync(new List<DeviceProperty>());

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetAvatarUrl(this.mockModelId))
                .ReturnsAsync(string.Empty);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.UpdateDeviceModel(It.IsAny<DeviceModelDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.SetDeviceModelModelProperties(It.IsAny<string>(), It.Is<List<DeviceProperty>>(properties => properties.Count.Equals(1))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns(value: null);

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

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnRemovePropertyShouldRemoveTheProperty()
        {
            // Arrange

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(this.mockModelId))
                .ReturnsAsync(new DeviceModelDto
                {
                    ModelId = this.mockModelId,
                    Name = Guid.NewGuid().ToString()
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(this.mockModelId))
                .ReturnsAsync(new[]
                {
                    new DeviceProperty()
                });

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetAvatarUrl(this.mockModelId))
                .ReturnsAsync(string.Empty);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.UpdateDeviceModel(It.IsAny<DeviceModelDto>()))
                .Returns(Task.CompletedTask);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.SetDeviceModelModelProperties(It.IsAny<string>(), It.Is<List<DeviceProperty>>(properties => properties.Count.Equals(0))))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var saveButton = cut.WaitForElement("#saveButton");
            var removePropertyButton = cut.WaitForElement("#DeletePropertyButton");
            removePropertyButton.Click();

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenPresentModelDetailsShouldDisplayProperties()
        {
            // Arrange

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

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
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureIsDisabledModelDetailsShouldNotDisplayLoRaWANTab()
        {
            // Arrange
            _ = SetupMockDeviceModel();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            cut.WaitForAssertion(() => cut.Find("#form"));

            var tabs = cut.WaitForElements(".mud-tabs .mud-tab");
            Assert.AreEqual(1, tabs.Count);
            Assert.AreEqual("General", tabs.Single().TextContent);

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenLoraFeatureIsEnabledModelDetailsShouldDisplayLoRaWANTab()
        {
            // Arrange
            _ = SetupMockLoRaWANDeviceModel();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

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
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingDeviceModel()
        {
            // Arrange
            _ = SetupMockLoRaWANDeviceModelThrowingException();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });


            // Act
            var cut = RenderComponent<DeviceModelDetailPage>(
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId),
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.IsLoRa), true));

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        private DeviceModelDto SetupMockDeviceModel(DeviceProperty[] properties = null)
        {
            var deviceModel = new DeviceModelDto
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = new Uri($"http://fake.local/{this.mockModelId}"),
                SupportLoRaFeatures = false
            };

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(this.mockModelId))
                .ReturnsAsync(deviceModel);

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelModelProperties(this.mockModelId))
                .ReturnsAsync(properties ?? Array.Empty<DeviceProperty>());

            _ = this.mockDeviceModelsClientService.Setup(service =>
                    service.GetAvatarUrl(this.mockModelId))
                .ReturnsAsync(deviceModel.ImageUrl.ToString());

            return deviceModel;
        }

        private LoRaDeviceModelDto SetupMockLoRaWANDeviceModel(DeviceModelCommandDto[] commands = null)
        {
            var deviceModel = new LoRaDeviceModelDto
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = new Uri($"http://fake.local/{this.mockModelId}")
            };

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(this.mockModelId))
                .ReturnsAsync(deviceModel);

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModelCommands(this.mockModelId))
                .ReturnsAsync(commands ?? Array.Empty<DeviceModelCommandDto>());

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service =>
                    service.GetAvatarUrl(this.mockModelId))
                .ReturnsAsync(deviceModel.ImageUrl.ToString());

            return deviceModel;
        }

        private LoRaDeviceModelDto SetupMockLoRaWANDeviceModelThrowingException()
        {
            var deviceModel = new LoRaDeviceModelDto
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = new Uri($"http://fake.local/{this.mockModelId}")
            };

            _ = this.mockLoRaWanDeviceModelsClientService.Setup(service =>
                    service.GetDeviceModel(this.mockModelId))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            return deviceModel;
        }

        [Test]
        public void ReturnButtonMustNavigateToPreviousPage()
        {

            // Arrange
            _ = SetupMockDeviceModel();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>(ComponentParameter.CreateParameter("ModelId", this.mockModelId ));
            var returnButton = cut.WaitForElement("#returnButton");

            returnButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndReturnIfAborted()
        {
            // Arrange
            _ = SetupMockDeviceModel();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Cancel());

            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnDeleteShouldDisplayConfirmationDialogAndRedirectIfConfirmed()
        {
            // Arrange
            _ = SetupMockDeviceModel();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "Azure" });

            var mockDialogReference = MockRepository.Create<IDialogReference>();
            _ = mockDialogReference.Setup(c => c.Result).ReturnsAsync(DialogResult.Ok("Ok"));
            _ = this.mockDialogService.Setup(c => c.Show<DeleteDeviceModelPage>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
                .Returns(mockDialogReference.Object);

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), this.mockModelId));

            var deleteButton = cut.WaitForElement("#deleteButton");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

    }

}
