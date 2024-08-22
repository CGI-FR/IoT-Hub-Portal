// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using UnitTests.Bases;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Portal.Client.Services;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;

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
        public void DuplicateSharedDeviceShouldReturnDuplicatedLoraWanDevice()
        {
            // Arrange
            var deviceId = Fixture.Create<string>();
            var deviceName = Fixture.Create<string>();
            var appKey = Fixture.Create<string>();

            // Act
            var loraWanDevice = this.deviceLayoutService.DuplicateSharedDevice(new LoRaDeviceDetails
            {
                DeviceID = deviceId,
                DeviceName = deviceName,
                AppKey = appKey
            });

            // Assert
            _ = loraWanDevice.DeviceID.Should().BeEmpty();
            _ = loraWanDevice.DeviceName.Should().Be($"{deviceName} - copy");
            _ = loraWanDevice.AppKey.Should().BeEmpty();
        }

        [Test]
        public void DuplicateSharedDeviceModelShouldReturnDuplicatedDeviceModel()
        {
            // Arrange
            var deviceModel = Fixture.Create<DeviceModelDto>();

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
            var result = this.deviceLayoutService.ResetSharedDevice<DeviceDetails>();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
        }

        [Test]
        public void ResetSharedDeviceShouldReturnNewDeviceWithExpectedTags()
        {
            // Arrange
            var expectedTags = Fixture.CreateMany<DeviceTagDto>(2).ToList();

            var expectedDevice = new DeviceDetails();

            foreach (var tag in expectedTags)
            {
                _ = expectedDevice.Tags.TryAdd(tag.Name, string.Empty);
            }

            // Act
            var result = this.deviceLayoutService.ResetSharedDevice<DeviceDetails>(expectedTags);

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDevice);
        }

        [Test]
        public void ResetSharedDeviceModelShouldReturnNewDeviceModel()
        {
            // Arrange
            var expectedDeviceModel = new DeviceModelDto();

            // Act
            var result = this.deviceLayoutService.ResetSharedDeviceModel<DeviceModelDto>();

            // Assert
            _ = result.Should().BeEquivalentTo(expectedDeviceModel);
        }

        [Test]
        public void GetSharedDeviceShouldReturnNull()
        {
            // Assert
            Assert.IsNull(this.deviceLayoutService.GetSharedDevice());
        }

        [Test]
        public void GetSharedDeviceModelShouldReturnNull()
        {
            // Assert
            Assert.IsNull(this.deviceLayoutService.GetSharedDeviceModel());
        }
    }
}
