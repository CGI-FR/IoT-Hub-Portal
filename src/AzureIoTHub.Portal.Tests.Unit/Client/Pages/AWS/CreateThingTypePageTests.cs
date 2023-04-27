// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.AWS
{

    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Client.Exceptions;
    using AzureIoTHub.Portal.Client.Models;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels;
    using AzureIoTHub.Portal.Client.Services;
    using Models.v10;
    using UnitTests.Bases;
    using Bunit;
    using Bunit.TestDoubles;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using UnitTests.Mocks;
    using AzureIoTHub.Portal.Client.Services.AWS;
    using AzureIoTHub.Portal.Models.v10.AWS;

    [TestFixture]
    public class CreateThingTypePageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<IDeviceModelsClientService> mockDeviceModelsClientService;
        private Mock<IThingTypeClientService> mockThingTypeClientService;
        private Mock<ILoRaWanDeviceModelsClientService> mockLoRaWanDeviceModelsClientService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();
            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockDeviceModelsClientService = MockRepository.Create<IDeviceModelsClientService>();
            this.mockThingTypeClientService = MockRepository.Create<IThingTypeClientService>();
            this.mockLoRaWanDeviceModelsClientService = MockRepository.Create<ILoRaWanDeviceModelsClientService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
            _ = Services.AddSingleton(this.mockSnackbarService.Object);
            _ = Services.AddSingleton(this.mockDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockThingTypeClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);
            _ = Services.AddSingleton(this.mockLoRaWanDeviceModelsClientService.Object);

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));
        }

        [Test]
        public void ClickOnSaveShouldPostThingType()
        {
            // Arrange
            var thingTypeName = Guid.NewGuid().ToString();
            var ThingTypeDescription = Guid.NewGuid().ToString();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });
            _ = this.mockThingTypeClientService.Setup(service =>
                    service.CreateThingType(It.Is<ThingTypeDetails>(thingType =>
                        thingTypeName.Equals(thingType.ThingTypeName, StringComparison.Ordinal)
                        && ThingTypeDescription.Equals(thingType.ThingTypeDescription, StringComparison.Ordinal)
                        )))
                .Returns(Task.CompletedTask);


            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(ThingTypeDetails.ThingTypeName)}").Change(thingTypeName);
            cut.WaitForElement($"#{nameof(ThingTypeDetails.ThingTypeDescription)}").Change(ThingTypeDescription);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnSaveShouldProcessProblemDetailsExceptionWhenIssueOccursOnCreatingThingType()
        {
            // Arrange
            var thingTypeName = Guid.NewGuid().ToString();
            var thingTypeDescription = Guid.NewGuid().ToString();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            _ = this.mockThingTypeClientService.Setup(service =>
                    service.CreateThingType(It.Is<ThingTypeDetails>(thingType =>
                        thingTypeName.Equals(thingType.ThingTypeName, StringComparison.Ordinal)
                        && thingTypeDescription.Equals(thingType.ThingTypeDescription, StringComparison.Ordinal))))
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");

            cut.WaitForElement($"#{nameof(ThingTypeDetails.ThingTypeName)}").Change(thingTypeName);
            cut.WaitForElement($"#{nameof(ThingTypeDetails.ThingTypeDescription)}").Change(thingTypeDescription);

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().NotEndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddTagShouldAddNewTag()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToString();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            _ = this.mockThingTypeClientService.Setup(service =>
                    service.CreateThingType(It.IsAny<ThingTypeDetails>()))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");
            var addPropertyButton = cut.WaitForElement("#addTagButton");

            cut.WaitForElement($"#{nameof(ThingTypeDetails.ThingTypeName)}").Change(Guid.NewGuid().ToString());
            cut.WaitForElement($"#{nameof(ThingTypeDetails.ThingTypeDescription)}").Change(Guid.NewGuid().ToString());

            addPropertyButton.Click();

            cut.WaitForElement($"#tag- #{nameof(ThingTypeTagDetails.Key)}").Change(key);

            var propertyCssSelector = $"#tag-{key}";

            cut.WaitForElement($"{propertyCssSelector} #{nameof(ThingTypeTagDetails.Key)}").Change(key);
            cut.WaitForElement($"{propertyCssSelector} #{nameof(ThingTypeTagDetails.Value)}").Change(value);

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll("#deleteTagButton").Count));

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickOnAddSearchableAttributeShouldAddNewSearchableAttr()
        {
            // Arrange
            var name = Guid.NewGuid().ToString();

            _ = Services.AddSingleton(new PortalSettings { CloudProvider = "AWS" });

            _ = this.mockThingTypeClientService.Setup(service =>
                    service.CreateThingType(It.IsAny<ThingTypeDetails>()))
                .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>())).Returns((Snackbar)null);

            // Act
            var cut = RenderComponent<CreateDeviceModelPage>();
            var saveButton = cut.WaitForElement("#SaveButton");
            var addPropertyButton = cut.WaitForElement("#addSearchableAttButton");

            cut.WaitForElement($"#{nameof(ThingTypeDetails.ThingTypeName)}").Change(Guid.NewGuid().ToString());
            cut.WaitForElement($"#{nameof(ThingTypeDetails.ThingTypeDescription)}").Change(Guid.NewGuid().ToString());

            addPropertyButton.Click();

            cut.WaitForElement($"#search- #{nameof(ThingTypeSearchableAttDto.Name)}").Change(name);

            var propertyCssSelector = $"#search-{name}";

            cut.WaitForElement($"{propertyCssSelector} #{nameof(ThingTypeSearchableAttDto.Name)}").Change(name);

            cut.WaitForAssertion(() => Assert.AreEqual(1, cut.FindAll("#deleteSearchableButton").Count));

            saveButton.Click();

            // Assert
            cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/device-models"));
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
