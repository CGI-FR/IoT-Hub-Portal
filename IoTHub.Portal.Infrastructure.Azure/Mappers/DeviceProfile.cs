// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Azure.Helpers;
    using IoTHub.Portal.Infrastructure.Azure.Mappers.Resolvers;
    using IoTHub.Portal.Models.v10;
    using Microsoft.Azure.Devices.Shared;
    using Models.v10.LoRaWAN;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    public class DeviceProfile : Profile
    {
        public DeviceProfile()
        {
            _ = CreateMap<Twin, Device>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetailsDto.DeviceName))))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetailsDto.ModelId))))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)));

            _ = CreateMap<Twin, LorawanDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetailsDto.DeviceName))))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetailsDto.ModelId))))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)))
                .ForMember(dest => dest.UseOTAA, opts => opts.MapFrom(src => !string.IsNullOrEmpty(DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.AppEUI)))))
                .ForMember(dest => dest.AppKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.AppKey))))
                .ForMember(dest => dest.AppEUI, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.AppEUI))))
                .ForMember(dest => dest.AppSKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.AppSKey))))
                .ForMember(dest => dest.NwkSKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.NwkSKey))))
                .ForMember(dest => dest.DevAddr, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.DevAddr))))
                .ForMember(dest => dest.AlreadyLoggedInOnce, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, "DevAddr") != null))
                .ForMember(dest => dest.DataRate, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.DataRate))))
                .ForMember(dest => dest.TxPower, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.TxPower))))
                .ForMember(dest => dest.NbRep, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.NbRep))))
                .ForMember(dest => dest.ReportedRX2DataRate, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.ReportedRX2DataRate))))
                .ForMember(dest => dest.ReportedRX1DROffset, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.ReportedRX1DROffset))))
                .ForMember(dest => dest.ReportedRXDelay, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.ReportedRXDelay))))
                .ForMember(dest => dest.GatewayID, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.GatewayID))))
                .ForMember(dest => dest.Downlink, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetailsDto.Downlink))))
                .ForMember(dest => dest.RX1DROffset, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.RX1DROffset))))
                .ForMember(dest => dest.RX2DataRate, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.RX2DataRate))))
                .ForMember(dest => dest.RXDelay, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.RXDelay))))
                .ForMember(dest => dest.ABPRelaxMode, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetailsDto.ABPRelaxMode))))
                .ForMember(dest => dest.FCntUpStart, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.FCntUpStart))))
                .ForMember(dest => dest.FCntDownStart, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.FCntDownStart))))
                .ForMember(dest => dest.FCntResetCounter, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.FCntResetCounter))))
                .ForMember(dest => dest.Supports32BitFCnt, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetailsDto.Supports32BitFCnt))))
                .ForMember(dest => dest.KeepAliveTimeout, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.KeepAliveTimeout))))
                .ForMember(dest => dest.SensorDecoder, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.SensorDecoder))))
                .ForMember(dest => dest.Deduplication, opts => opts.MapFrom(src => GetDesiredPropertyAsEnum<DeduplicationMode>(src, nameof(LoRaDeviceDetailsDto.Deduplication))))
                .ForMember(dest => dest.PreferredWindow, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.PreferredWindow)) ?? 0))
                .ForMember(dest => dest.ClassType, opts => opts.MapFrom(src => GetDesiredPropertyAsEnum<ClassType>(src, nameof(LoRaDeviceDetailsDto.ClassType))));

            _ = CreateMap<Twin, DeviceDetailsDto>()
                .ForMember(dest => dest.DeviceID, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetailsDto.DeviceName))))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetailsDto.ModelId))))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)))
                .ForMember(dest => dest.ImageUrl, opts => opts.MapFrom<DeviceImageUrlValueResolver>());

            _ = CreateMap<DeviceDetailsDto, Twin>()
                .ForMember(dest => dest.DeviceId, opts => opts.MapFrom(src => src.DeviceID))
                .ForMember(dest => dest.Tags, opts => opts.Ignore())
                .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(DeviceDetailsDto.DeviceName), src.DeviceName))
                .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(DeviceDetailsDto.ModelId), src.ModelId));

            _ = CreateMap<Twin, LoRaDeviceDetailsDto>()
                .ForMember(dest => dest.DeviceID, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetailsDto.DeviceName))))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetailsDto.ModelId))))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.UseOTAA, opts => opts.MapFrom(src => !string.IsNullOrEmpty(DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.AppEUI)))))
                .ForMember(dest => dest.AppKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.AppKey))))
                .ForMember(dest => dest.AppEUI, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.AppEUI))))
                .ForMember(dest => dest.AppSKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.AppSKey))))
                .ForMember(dest => dest.NwkSKey, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.NwkSKey))))
                .ForMember(dest => dest.DevAddr, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.DevAddr))))
                .ForMember(dest => dest.AlreadyLoggedInOnce, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, "DevAddr") != null))
                .ForMember(dest => dest.DataRate, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.DataRate))))
                .ForMember(dest => dest.TxPower, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.TxPower))))
                .ForMember(dest => dest.NbRep, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.NbRep))))
                .ForMember(dest => dest.ReportedRX2DataRate, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.ReportedRX2DataRate))))
                .ForMember(dest => dest.ReportedRX1DROffset, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.ReportedRX1DROffset))))
                .ForMember(dest => dest.ReportedRXDelay, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.ReportedRXDelay))))
                .ForMember(dest => dest.GatewayID, opts => opts.MapFrom(src => DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetailsDto.GatewayID))))
                .ForMember(dest => dest.Downlink, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetailsDto.Downlink))))
                .ForMember(dest => dest.RX1DROffset, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.RX1DROffset))))
                .ForMember(dest => dest.RX2DataRate, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.RX2DataRate))))
                .ForMember(dest => dest.RXDelay, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.RXDelay))))
                .ForMember(dest => dest.ABPRelaxMode, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetailsDto.ABPRelaxMode))))
                .ForMember(dest => dest.FCntUpStart, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.FCntUpStart))))
                .ForMember(dest => dest.FCntDownStart, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.FCntDownStart))))
                .ForMember(dest => dest.FCntResetCounter, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.FCntResetCounter))))
                .ForMember(dest => dest.Supports32BitFCnt, opts => opts.MapFrom(src => GetDesiredPropertyAsBooleanValue(src, nameof(LoRaDeviceDetailsDto.Supports32BitFCnt))))
                .ForMember(dest => dest.KeepAliveTimeout, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.KeepAliveTimeout))))
                .ForMember(dest => dest.SensorDecoder, opts => opts.MapFrom(src => DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetailsDto.SensorDecoder))))
                .ForMember(dest => dest.Deduplication, opts => opts.MapFrom(src => GetDesiredPropertyAsEnum<DeduplicationMode>(src, nameof(LoRaDeviceDetailsDto.Deduplication))))
                .ForMember(dest => dest.PreferredWindow, opts => opts.MapFrom(src => GetDesiredPropertyAsIntegerValue(src, nameof(LoRaDeviceDetailsDto.PreferredWindow)) ?? 0))
                .ForMember(dest => dest.ClassType, opts => opts.MapFrom(src => GetDesiredPropertyAsEnum<ClassType>(src, nameof(LoRaDeviceDetailsDto.ClassType))));

            _ = CreateMap<LoRaDeviceDetailsDto, Twin>()
               .AfterMap((src, dest) => dest.DeviceId = dest.DeviceId)
               .ForMember(dest => dest.Tags, opts => opts.Ignore())
               .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(LoRaDeviceDetailsDto.DeviceName), src.DeviceName))
               .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(LoRaDeviceDetailsDto.ModelId), src.ModelId))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.AppKey), src.AppKey))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.AppEUI), src.AppEUI))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.AppSKey), src.AppSKey))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.DevAddr), src.DevAddr))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.Downlink), src.Downlink))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.RX1DROffset), src.RX1DROffset))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.RX2DataRate), src.RX2DataRate))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.RXDelay), src.RXDelay))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.ABPRelaxMode), src.ABPRelaxMode))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.FCntUpStart), src.FCntUpStart))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.FCntDownStart), src.FCntDownStart))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.FCntResetCounter), src.FCntResetCounter))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.Supports32BitFCnt), src.Supports32BitFCnt))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.KeepAliveTimeout), src.KeepAliveTimeout))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.SensorDecoder), src.SensorDecoder))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.Deduplication), src.Deduplication))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.PreferredWindow), src.PreferredWindow))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetailsDto.ClassType), src.ClassType));
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
