// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Client.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Models.v10;
    using NUnit.Framework;

    [TestFixture]
    public class DeviceLayoutServiceTests : BlazorUnitTest
    {
        private IDeviceLayoutService deviceLayoutService;

        public override void Setup()
        {
            base.Setup();

            _ = Services.AddSingleton<IDeviceLayoutService, DeviceLayoutService>();

            this.deviceLayoutService = Services.GetRequiredService<IDeviceLayoutService>();
        }

        [Test]
        public void RefreshDeviceShouldRaiseRefreshDeviceOccurredEvent()
        {
            // Arrange
            var receivedEvents = new List<string>();
            this.deviceLayoutService.RefreshDeviceOccurred += (sender, _) =>
            {
                receivedEvents.Add(sender?.GetType().ToString());
            };

            // Act
            this.deviceLayoutService.RefreshDevice();

            // Assert
            _ = receivedEvents.Count.Should().Be(1);
            _ = receivedEvents.First().Should().Be(typeof(DeviceLayoutService).ToString());
        }

        [Test]
        public void DuplicateSharedDeviceShouldReturnDuplicatedDevice()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var deviceName = Fixture.Create<string>();

            // Act
            var result = this.deviceLayoutService.DuplicateSharedDevice(new DeviceDetails
            {
                DeviceID = deviceId,
                DeviceName = deviceName
            });

            // Assert
            _ = result.DeviceID.Should().BeEmpty();
            _ = result.DeviceName.Should().Be($"{deviceName} - copy");
        }

        [Test]
        public void DuplicateSharedDeviceModelShouldReturnDuplicatedDeviceModel()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModel>();

            // Act
            var result = this.deviceLayoutService.DuplicateSharedDeviceModel(deviceModel);

            // Assert
            _ = result.Should().BeEquivalentTo(deviceModel);
        }

        [Test]
        public void ResetSharedDeviceShouldReturnNewDevice()
        {
            // Arrange
            var expectedDevice = new DeviceDetails();

            // Act
            var result = this.deviceLayoutService.ResetSharedDevice();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
        }

        [Test]
        public void ResetSharedDeviceModelShouldReturnNewDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = new DeviceModel();

            // Act
            var result = this.deviceLayoutService.ResetSharedDeviceModel();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModel);
        }

        [Test]
        public void GetSharedDeviceShouldReturnDevice()
        {
            // Arrange
            var expectedDevice = new DeviceDetails();

            // Act
            var result = this.deviceLayoutService.GetSharedDevice();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
        }

        [Test]
        public void GetSharedDeviceModelShouldReturnDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = new DeviceModel();

            // Act
            var result = this.deviceLayoutService.GetSharedDeviceModel();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModel);
        }
    }
}
