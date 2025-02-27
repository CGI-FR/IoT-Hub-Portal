// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.DevicesModels.LoRaWAN
{
    public class EditLoraDeviceModelPageTests : BlazorUnitTest
    {
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void CreateLoRaDeviceModelPageShouldBeRenderedProperly()
        {
            // Arrange
            var model = new LoRaDeviceModelDto();
            var commands = new List<DeviceModelCommandDto>();

            var cut = RenderComponent<EditLoraDeviceModel>(
                ComponentParameter.CreateParameter("LoRaDeviceModel", model),
                ComponentParameter.CreateParameter("Commands", commands)
            );

            cut.WaitForAssertion(() => cut.FindAll(".mud-expand-panel").Count.Should().Be(3));
        }

        [Test]
        public void ClickOnAddShouldAddRow()
        {
            // Arrange
            var model = new LoRaDeviceModelDto();
            var commands = new List<DeviceModelCommandDto>();

            var cut = RenderComponent<EditLoraDeviceModel>(
                ComponentParameter.CreateParameter("LoRaDeviceModel", model),
                ComponentParameter.CreateParameter("Commands", commands)
            );

            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(0));
            var addButton = cut.WaitForElement("#addButton");
            addButton.Click();
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
        }

        [Test]
        public void ClickOnRemoveShouldDeleteRow()
        {
            // Arrange
            var model = new LoRaDeviceModelDto();
            var commands = new List<DeviceModelCommandDto>();

            var cut = RenderComponent<EditLoraDeviceModel>(
                ComponentParameter.CreateParameter("LoRaDeviceModel", model),
                ComponentParameter.CreateParameter("Commands", commands)
            );

            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(0));
            var addButton = cut.WaitForElement("#addButton");
            addButton.Click();
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(1));
            var removeButton = cut.WaitForElement("#removeButton");
            removeButton.Click();
            cut.WaitForAssertion(() => cut.FindAll("table tbody tr").Count.Should().Be(0));
        }
    }
}
