// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using AutoMapper;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Domain.Entities;
    using Microsoft.Azure.Devices.Shared;
    using Shared.Models.v1._0;
    using Shared.Models.v1._0.LoRaWAN;

    public class DeviceProfile : Profile
    {
        public DeviceProfile()
        {
            _ = CreateMap<Device, Device>();

            _ = CreateMap<DeviceDetails, Device>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceID))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.DeviceName))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.Select(pair => new DeviceTagValue
                {
                    Name = pair.Key,
                    Value = pair.Value
                })));

            _ = CreateMap<Device, DeviceDetails>()
                .ForMember(dest => dest.DeviceID, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => src.DeviceModelId))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.ToDictionary(tag => tag.Name, tag => tag.Value)));

            _ = CreateMap<Twin, Device>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Tags["deviceName"]))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.Tags["modelId"]))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)));

            _ = CreateMap<LorawanDevice, LorawanDevice>();

            _ = CreateMap<LoRaDeviceDetails, LorawanDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceID))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.DeviceName))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.ModelId))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.Select(pair => new DeviceTagValue
                {
                    Name = pair.Key,
                    Value = pair.Value
                })));

            _ = CreateMap<LorawanDevice, LoRaDeviceDetails>()
                .ForMember(dest => dest.DeviceID, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => src.DeviceModelId))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => src.Tags.ToDictionary(tag => tag.Name, tag => tag.Value)));

            _ = CreateMap<Twin, LorawanDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Tags["deviceName"]))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.Tags["modelId"]))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)))
                .ForMember(dest => dest.UseOTAA, opts => opts.MapFrom(src => !string.IsNullOrEmpty(DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.AppEUI)))))
                .ForMember(dest => dest.AppKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.AppKey))))
                .ForMember(dest => dest.AppEUI, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.AppEUI))))
                .ForMember(dest => dest.AppSKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.AppSKey))))
                .ForMember(dest => dest.NwkSKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.NwkSKey))))
                .ForMember(dest => dest.DevAddr, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.DevAddr))))
                .ForMember(dest => dest.AlreadyLoggedInOnce, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, "DevAddr") != null))
                .ForMember(dest => dest.DataRate, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.DataRate))))
                .ForMember(dest => dest.TxPower, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.TxPower))))
                .ForMember(dest => dest.NbRep, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.NbRep))))
                .ForMember(dest => dest.ReportedRX2DataRate, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.ReportedRX2DataRate))))
                .ForMember(dest => dest.ReportedRX1DROffset, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.ReportedRX1DROffset))))
                .ForMember(dest => dest.ReportedRXDelay, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.ReportedRXDelay))))
                .ForMember(dest => dest.GatewayID, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.GatewayID))))
                .ForMember(dest => dest.Downlink, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetails.Downlink))))
                .ForMember(dest => dest.RX1DROffset, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetails.RX1DROffset))))
                .ForMember(dest => dest.RX2DataRate, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetails.RX2DataRate))))
                .ForMember(dest => dest.RXDelay, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetails.RXDelay))))
                .ForMember(dest => dest.ABPRelaxMode, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetails.ABPRelaxMode))))
                .ForMember(dest => dest.FCntUpStart, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetails.FCntUpStart))))
                .ForMember(dest => dest.FCntDownStart, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetails.FCntDownStart))))
                .ForMember(dest => dest.FCntResetCounter, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetails.FCntResetCounter))))
                .ForMember(dest => dest.Supports32BitFCnt, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetails.Supports32BitFCnt))))
                .ForMember(dest => dest.KeepAliveTimeout, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetails.KeepAliveTimeout))))
                .ForMember(dest => dest.SensorDecoder, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.SensorDecoder))))
                .ForMember(dest => dest.Deduplication, opts => opts.MapFrom(src => GetDesiredPropertyAsEnum<DeduplicationMode>(src, nameof(LoRaDeviceDetails.Deduplication))))
                .ForMember(dest => dest.PreferredWindow, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetails.PreferredWindow)) ?? 0))
                .ForMember(dest => dest.ClassType, opts => opts.MapFrom(src => GetDesiredPropertyAsEnum<ClassType>(src, nameof(LoRaDeviceDetails.ClassType))));

            _ = CreateMap<LoRaTelemetry, LoRaTelemetryDto>();
            _ = CreateMap<LoRaDeviceTelemetry, LoRaDeviceTelemetryDto>();
        }

        private static ICollection<DeviceTagValue> GetTags(Twin twin)
        {
            return (JsonSerializer.Deserialize<Dictionary<string, object>>(twin.Tags.ToJson()) ?? new Dictionary<string, object>())
                .Where(tag => tag.Key is not "modelId" and not "deviceName")
                .Select(tag => new DeviceTagValue
                {
                    Name = tag.Key,
                    Value = tag.Value.ToString() ?? string.Empty
                })
                .ToList();
        }

        private static bool? GetDesiredPropertyAsBooleanValue(Twin twin, string propertyName)
        {
            return bool.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, propertyName), out var boolResult) ? boolResult : null;
        }

        private static int? GetDesiredPropertyAsIntegerValue(Twin twin, string propertyName)
        {
            return int.TryParse(DeviceHelper.RetrieveDesiredPropertyValue(twin, propertyName), out var numberStyles) ? numberStyles : null;
        }

        private static TEnum GetDesiredPropertyAsEnum<TEnum>(Twin twin, string propertyName) where TEnum : struct, IConvertible
        {
            return Enum.TryParse<TEnum>(DeviceHelper.RetrieveDesiredPropertyValue(twin, propertyName), out var result) ? result : default;
        }
    }
}
