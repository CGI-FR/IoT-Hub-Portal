// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using System;
    using System.Collections.Generic;
    using AngleSharp.Dom;
    using AutoFixture;
    using IoTHub.Portal.Client.Components.Devices.LoRaWAN;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Client.Validators;
    using IoTHub.Portal.Models.v10.LoRaWAN;
    using IoTHub.Portal.Shared.Models.v10;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using UnitTests.Bases;
    using UnitTests.Mocks;

    [TestFixture]
    public class CreateLoraDeviceTests : BlazorUnitTest
    {
        private Mock<ILoRaWanDeviceClientService> mockLoRaWanDeviceClientService;

        public override void Setup()
        {
            base.Setup();
            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();

            _ = Services.AddSingleton(this.mockLoRaWanDeviceClientService.Object);

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));
        }

        [Test]
        public void WhenUseOTAAShouldDisplayOTAATextboxes()
        {
            var mockLoRaModel = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = true
            };

            var deviceDetails = new LoRaDeviceDetails();
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(new LoRaGatewayIDList());

            // Act

            var cut = RenderComponent<CreateLoraDevice>(
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraValidator), validator));


            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.AppEUI, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.AppEUI)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.AppKey, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.AppKey)}").GetAttribute("value")));
        }

        [Test]
        public void WhenNotUseOTAAShouldDisplayABPTextboxes()
        {
            var mockLoRaModel = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = false
            };

            var deviceDetails= new LoRaDeviceDetails();
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(new LoRaGatewayIDList());

            // Act
            var cut = RenderComponent<CreateLoraDevice>(
            ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoRaDevice), deviceDetails),
            ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraModelDto), mockLoRaModel),
            ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraValidator), validator));


            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.AppSKey, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.AppSKey)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.NwkSKey, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.NwkSKey)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.DevAddr, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.DevAddr)}").GetAttribute("value")));
        }

        [Test]
        public void CreateLoRaDevicePageShouldBeRenderedProperly()
        {
            // Arrange
            var mockLoRaModel = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = false
            };

            var deviceDetails= new LoRaDeviceDetails();
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var cut = RenderComponent<CreateLoraDevice>(
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraValidator), validator));

            // Assert   
            cut.WaitForAssertion(() => cut.FindAll(".mud-expand-panel").Count.Should().Be(1));
            Assert.AreEqual(3, cut.Instance.GatewayIdList.Count);
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void OnInitializedAsyncShouldProcessProblemDetailsExceptionWhenIssueOccursOnGettingGatewayIDList()
        {
            var mockLoRaModel = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = false
            };

            var deviceDetails= new LoRaDeviceDetails();
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<CreateLoraDevice>(
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraValidator), validator));

            // Assert
            Assert.AreEqual(0, cut.Instance.GatewayIdList.Count);
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void ClickingOnMudAutocompleteShouldDisplaySearch()
        {
            // Arrange
            var mockLoRaModel = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = false
            };

            var deviceDetails= new LoRaDeviceDetails();
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<CreateLoraDevice>(
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraValidator), validator));
            var autocompleteComponent = cut.FindComponent<MudAutocomplete<string>>();

            // Act
            autocompleteComponent.Find(TagNames.Input).Click();

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(3));
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeTrue());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void TypingOnMudAutocompleteShouldTriggerSearch()
        {
            // Arrange
            var query = "Gateway";

            var mockLoRaModel = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = false
            };

            var deviceDetails= new LoRaDeviceDetails();
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(new LoRaGatewayIDList()
                {
                    GatewayIds = new List<string>()
                    {
                        "GatewayID01", "GatewayID02", "TestValue"
                    }
                });

            // Act
            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<CreateLoraDevice>(
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraValidator), validator));
            var autocompleteComponent = cut.FindComponent<MudAutocomplete<string>>();

            // Act
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(3));
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeTrue());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
