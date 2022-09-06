// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Client.Pages.Devices
{
    using System;
    using AzureIoTHub.Portal.Client.Pages.Devices.LoRaWAN;
    using AzureIoTHub.Portal.Client.Validators;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Bunit;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor.Services;
    using NUnit.Framework;
    using UnitTests.Bases;
    using UnitTests.Mocks;

    [TestFixture]
    public class CreateLoraDeviceTests : BlazorUnitTest
    {

        public override void Setup()
        {
            base.Setup();

            Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));
        }

        [Test]
        public void WhenUseOTAAShouldDisplayOTAATextboxes()
        {
            var mockLoRaModel = new LoRaDeviceModel
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = true
            };

            var deviceDetails = new LoRaDeviceDetails()
            {
                DeviceID = Guid.NewGuid().ToString(),
                ModelId = mockLoRaModel.ModelId,
                AppEUI = Guid.NewGuid().ToString(),
                AppKey = Guid.NewGuid().ToString()
            };

            var validator = new LoRaDeviceDetailsValidator();

            // Act

            var cut = RenderComponent<CreateLoraDevice>(
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.loraModel), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraValidator), validator));


            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.AppEUI, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.AppEUI)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.AppKey, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.AppKey)}").GetAttribute("value")));
        }

        [Test]
        public void WhenNotUseOTAAShouldDisplayABPTextboxes()
        {
            var mockLoRaModel = new LoRaDeviceModel
            {
                ModelId = Guid.NewGuid().ToString(),
                UseOTAA = false
            };

            var deviceDetails= new LoRaDeviceDetails()
            {
                DeviceID = Guid.NewGuid().ToString(),
                ModelId = mockLoRaModel.ModelId,
                AppSKey = Guid.NewGuid().ToString(),
                NwkSKey = Guid.NewGuid().ToString(),
                DevAddr = Guid.NewGuid().ToString(),
            };
            var validator = new LoRaDeviceDetailsValidator();

            // Act
            var cut = RenderComponent<CreateLoraDevice>(
            ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoRaDevice), deviceDetails),
            ComponentParameter.CreateParameter(nameof(CreateLoraDevice.loraModel), mockLoRaModel),
            ComponentParameter.CreateParameter(nameof(CreateLoraDevice.LoraValidator), validator));


            // Assert

            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.AppSKey, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.AppSKey)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.NwkSKey, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.NwkSKey)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.DevAddr, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.DevAddr)}").GetAttribute("value")));
        }
    }
}
