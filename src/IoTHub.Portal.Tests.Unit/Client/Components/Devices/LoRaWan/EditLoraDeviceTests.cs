// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Components.Devices.LoRaWan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using AutoFixture;
    using IoTHub.Portal.Client.Components.Devices.LoRaWAN;
    using IoTHub.Portal.Client.Exceptions;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Client.Services;
    using IoTHub.Portal.Client.Validators;
    using IoTHub.Portal.Shared.Models.v1._0;
    using Bunit;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using MudBlazor;
    using MudBlazor.Services;
    using NUnit.Framework;
    using Shared.Models.v1._0.LoRaWAN;
    using UnitTests.Bases;
    using UnitTests.Mocks;

    [TestFixture]
    public class EditLoraDeviceTests : BlazorUnitTest
    {
        private Mock<ISnackbar> mockSnackbarService;
        private Mock<ILoRaWanDeviceClientService> mockLoRaWanDeviceClientService;
        public override void Setup()
        {
            base.Setup();

            this.mockSnackbarService = MockRepository.Create<ISnackbar>();
            this.mockLoRaWanDeviceClientService = MockRepository.Create<ILoRaWanDeviceClientService>();

            _ = Services.AddSingleton(this.mockSnackbarService.Object);
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

            var deviceDetails = new LoRaDeviceDetails()
            {
                DeviceID = Guid.NewGuid().ToString(),
                ModelId = mockLoRaModel.ModelId,
                AppEUI = Guid.NewGuid().ToString(),
                AppKey = Guid.NewGuid().ToString()
            };

            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoraValidator), validator));


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

            var deviceDetails= new LoRaDeviceDetails()
            {
                DeviceID = Guid.NewGuid().ToString(),
                ModelId = mockLoRaModel.ModelId,
                AppSKey = Guid.NewGuid().ToString(),
                NwkSKey = Guid.NewGuid().ToString(),
                DevAddr = Guid.NewGuid().ToString(),
            };
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoraValidator), validator));

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.AppSKey, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.AppSKey)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.NwkSKey, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.NwkSKey)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.DevAddr, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.DevAddr)}").GetAttribute("value")));
        }

        [Test]
        public void EditLoRaDevicePageShouldBeRenderedProperly()
        {
            // Arrange

            var mockLoRaModel = new LoRaDeviceModelDto
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

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoraValidator), validator));

            // Assert   
            cut.WaitForAssertion(() => cut.FindAll(".mud-expand-panel").Count.Should().Be(3));
            Assert.AreEqual(deviceDetails.Deduplication, Enum.Parse<DeduplicationMode>(cut.WaitForElement($"#{nameof(LoRaDeviceDetails.Deduplication)}").GetAttribute("value")));
            Assert.AreEqual(deviceDetails.ClassType, Enum.Parse<ClassType>(cut.WaitForElement($"#{nameof(LoRaDeviceDetails.ClassType)}").GetAttribute("value")));
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

            var deviceDetails= new LoRaDeviceDetails()
            {
                DeviceID = Guid.NewGuid().ToString(),
                ModelId = mockLoRaModel.ModelId,
                AppSKey = Guid.NewGuid().ToString(),
                NwkSKey = Guid.NewGuid().ToString(),
                DevAddr = Guid.NewGuid().ToString(),
            };
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ThrowsAsync(new ProblemDetailsException(new ProblemDetailsWithExceptionDetails()));

            // Act
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoraValidator), validator));

            // Assert
            Assert.AreEqual(0, cut.Instance.GatewayIdList.Count);
            cut.WaitForAssertion(() => cut.Markup.Should().NotBeNullOrEmpty());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }

        [Test]
        public void WhenDeviceNeverConnectedCommandsShouldBeDisabled()
        {
            // Arrange
            var model = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
            };
            var commands = new List<DeviceModelCommandDto>
            {
                new DeviceModelCommandDto
                {
                    Name = Guid.NewGuid().ToString()
                }
            };

            var device = new LoRaDeviceDetails()
            {
                ModelId = model.ModelId
            };

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), device),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), model),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.Commands), commands));

            // Assert
            cut.WaitForState(() => cut.Find("#CommandNotAvailableMessage").TextContent.Contains("You cannot send command at this moment.", StringComparison.OrdinalIgnoreCase));
            cut.WaitForAssertion(() => cut.FindAll("#LoRaWANCommandsTable tbody tr").Count.Should().Be(1));
            Assert.IsTrue(cut.Find("#LoRaWANCommandsTable tbody tr:first-child #ExecuteCommand").HasAttribute("disabled"));
        }

        [Test]
        public void WhenDeviceAlteadyConnectedCommandsShouldBeEnabled()
        {
            // Arrange
            var model = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
            };
            var commands = new List<DeviceModelCommandDto>
            {
                new DeviceModelCommandDto
                {
                    Name = Guid.NewGuid().ToString()
                }
            };

            var device = new LoRaDeviceDetails()
            {
                ModelId = model.ModelId,
                AlreadyLoggedInOnce = true
            };

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), device),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), model),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.Commands), commands));

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#LoRaWANCommandsTable tbody tr").Count.Should().Be(1));
            Assert.IsFalse(cut.Find("#LoRaWANCommandsTable tbody tr:first-child #ExecuteCommand").HasAttribute("disabled"));
        }

        [Test]
        public void WhenClickToSendCommandShouldExecuteCommandToService()
        {
            // Arrange
            var model = new LoRaDeviceModelDto
            {
                ModelId = Guid.NewGuid().ToString(),
            };
            var commands = new List<DeviceModelCommandDto>
            {
                new DeviceModelCommandDto
                {
                    Name = Guid.NewGuid().ToString()
                }
            };

            var device = new LoRaDeviceDetails()
            {
                ModelId = model.ModelId,
                AlreadyLoggedInOnce = true
            };

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.ExecuteCommand(device.DeviceID, commands.Single().Id))
                            .Returns(Task.CompletedTask);

            _ = this.mockSnackbarService.Setup(c => c.Add(It.IsAny<string>(), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()))
                .Returns((Snackbar)null);

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), device),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), model),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.Commands), commands));

            // Assert
            cut.WaitForAssertion(() => cut.FindAll("#LoRaWANCommandsTable tbody tr").Count.Should().Be(1));
            cut.WaitForElement("#LoRaWANCommandsTable tbody tr:first-child #ExecuteCommand").Click();

            this.mockLoRaWanDeviceClientService.Verify(c => c.ExecuteCommand(device.DeviceID, commands.Single().Id), Times.Once);
            this.mockSnackbarService.Verify(c => c.Add(It.Is<string>(x => x.Contains($"{commands.Single().Name} has been successfully executed!", StringComparison.OrdinalIgnoreCase)), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void EditLoRaDeviceDetailPageWithReportedProperties()
        {
            // Arrange
            var mockLoRaModel = new LoRaDeviceModelDto
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

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoraValidator), validator));

            // Assert
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.DataRate, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.DataRate)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.TxPower, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.TxPower)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.NbRep, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.NbRep)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.ReportedRX1DROffset, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.ReportedRX1DROffset)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.ReportedRX2DataRate, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.ReportedRX2DataRate)}").GetAttribute("value")));
            cut.WaitForAssertion(() => Assert.AreEqual(deviceDetails.ReportedRXDelay, cut.WaitForElement($"#{nameof(LoRaDeviceDetails.ReportedRXDelay)}").GetAttribute("value")));
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

            var deviceDetails= new LoRaDeviceDetails()
            {
                DeviceID = Guid.NewGuid().ToString(),
                ModelId = mockLoRaModel.ModelId,
                AppSKey = Guid.NewGuid().ToString(),
                NwkSKey = Guid.NewGuid().ToString(),
                DevAddr = Guid.NewGuid().ToString()
            };
            var validator = new LoRaDeviceDetailsValidator();

            _ = this.mockLoRaWanDeviceClientService.Setup(c => c.GetGatewayIdList())
                .ReturnsAsync(Fixture.Create<LoRaGatewayIDList>);

            // Act
            var popoverProvider = RenderComponent<MudPopoverProvider>();
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoraValidator), validator));
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

            var deviceDetails= new LoRaDeviceDetails()
            {
                DeviceID = Guid.NewGuid().ToString(),
                ModelId = mockLoRaModel.ModelId,
                AppSKey = Guid.NewGuid().ToString(),
                NwkSKey = Guid.NewGuid().ToString(),
                DevAddr = Guid.NewGuid().ToString(),
            };
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
            var cut = RenderComponent<EditLoraDevice>(
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDevice), deviceDetails),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoRaDeviceModelDto), mockLoRaModel),
                ComponentParameter.CreateParameter(nameof(EditLoraDevice.LoraValidator), validator));
            var autocompleteComponent = cut.FindComponent<MudAutocomplete<string>>();

            // Act
            autocompleteComponent.Find(TagNames.Input).Click();
            autocompleteComponent.Find(TagNames.Input).Input(query);

            // Assert
            popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(2));
            cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeTrue());
            cut.WaitForAssertion(() => MockRepository.VerifyAll());
        }
    }
}
