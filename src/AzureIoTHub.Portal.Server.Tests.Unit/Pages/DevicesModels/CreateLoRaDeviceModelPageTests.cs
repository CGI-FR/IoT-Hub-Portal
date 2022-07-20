// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages.DevicesModels
{
    using System.Collections.Generic;
    using AzureIoTHub.Portal.Client.Pages.DeviceModels.LoRaWAN;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using NUnit.Framework;

    [TestFixture]
    public class CreateLoRaDeviceModelPageTests : BlazorUnitTest
    {
        private Mock<IDialogService> mockDialogService;

        public override void Setup()
        {
            base.Setup();

            this.mockDialogService = MockRepository.Create<IDialogService>();

            _ = Services.AddSingleton(this.mockDialogService.Object);
        }

        [Test]
        public void CreateLoRaDeviceModelPageShouldBeRenderedProperly()
        {
            // Arrange
            var model = new LoRaDeviceModel();
            var commands = new List<DeviceModelCommand>();
            var commandValidation = new List<MudForm>()
            {
                new MudForm(),
                new MudForm(),
                new MudForm()
            };

            var cut = RenderComponent<CreateLoraDeviceModel>(
                ComponentParameter.CreateParameter("LoRaDeviceModel", model),
                ComponentParameter.CreateParameter("Commands", commands),
                ComponentParameter.CreateParameter("CommandValidation", commandValidation)
            );

            cut.WaitForAssertion(() => cut.FindAll(".mud-expand-panel").Count.Should().Be(3));
        }

        [Test]
        public void ClickOnAddShouldAddRow()
        {
            // Arrange
            var model = new LoRaDeviceModel();
            var commands = new List<DeviceModelCommand>();
            var commandValidation = new List<MudForm>()
            {
                new MudForm(),
                new MudForm(),
                new MudForm()
            };

            var cut = RenderComponent<CreateLoraDeviceModel>(
                ComponentParameter.CreateParameter("LoRaDeviceModel", model),
                ComponentParameter.CreateParameter("Commands", commands),
                ComponentParameter.CreateParameter("CommandValidation", commandValidation)
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
            var model = new LoRaDeviceModel();
            var commands = new List<DeviceModelCommand>();
            var commandValidation = new List<MudForm>()
            {
                new MudForm(),
                new MudForm(),
                new MudForm()
            };

            var cut = RenderComponent<CreateLoraDeviceModel>(
                ComponentParameter.CreateParameter("LoRaDeviceModel", model),
                ComponentParameter.CreateParameter("Commands", commands),
                ComponentParameter.CreateParameter("CommandValidation", commandValidation)
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
