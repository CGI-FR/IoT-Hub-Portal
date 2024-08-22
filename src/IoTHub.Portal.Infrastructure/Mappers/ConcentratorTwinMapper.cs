// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Mappers
{
    using System;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Mappers;
    using IoTHub.Portal.Crosscutting.Extensions;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Shared.Models.v1._0.LoRaWAN;

    public class ConcentratorTwinMapper : IConcentratorTwinMapper
    {
        public ConcentratorDto CreateDeviceDetails(Twin twin)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));

            return new ConcentratorDto
            {
                DeviceId = twin.DeviceId,
                DeviceName = DeviceHelper.RetrieveTagValue(twin, nameof(ConcentratorDto.DeviceName))!,
                LoraRegion = DeviceHelper.RetrieveTagValue(twin, nameof(ConcentratorDto.LoraRegion))!,
                ClientThumbprint = DeviceHelper.RetrieveClientThumbprintValue(twin),
                IsEnabled = twin.Status == DeviceStatus.Enabled,
                IsConnected = twin.ConnectionState == DeviceConnectionState.Connected,
                AlreadyLoggedInOnce = DeviceHelper.RetrieveReportedPropertyValue(twin, "DevAddr") != null,
                DeviceType = DeviceHelper.RetrieveTagValue(twin, nameof(ConcentratorDto.DeviceType))!
            };
        }

        public void UpdateTwin(Twin twin, ConcentratorDto item)
        {
            ArgumentNullException.ThrowIfNull(twin, nameof(twin));
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            DeviceHelper.SetTagValue(twin, nameof(item.DeviceName), item.DeviceName);
            DeviceHelper.SetTagValue(twin, nameof(item.LoraRegion), item.LoraRegion);
            DeviceHelper.SetTagValue(twin, nameof(item.DeviceType), item.DeviceType);

            if (!string.IsNullOrWhiteSpace(item.ClientThumbprint))
            {
                DeviceHelper.SetDesiredProperty(twin, nameof(item.ClientThumbprint).ToCamelCase(), new[] { item.ClientThumbprint });
            }
            else
            {
                DeviceHelper.SetDesiredProperty(twin, nameof(item.ClientThumbprint).ToCamelCase(), null);
            }

            DeviceHelper.SetDesiredProperty(twin, nameof(item.RouterConfig).ToCamelCase(), item.RouterConfig);
        }
    }
}
