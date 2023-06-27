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
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetails.DeviceName))))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetails.ModelId))))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)));

            _ = CreateMap<Twin, LorawanDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetails.DeviceName))))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetails.ModelId))))
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

            _ = CreateMap<Twin, DeviceDetails>()
                .ForMember(dest => dest.DeviceID, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetails.DeviceName))))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetails.ModelId))))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)))
                .ForMember(dest => dest.ImageUrl, opts => opts.MapFrom<DeviceImageUrlValueResolver>());

            _ = CreateMap<DeviceDetails, Twin>()
                .ForMember(dest => dest.DeviceId, opts => opts.MapFrom(src => src.DeviceID))
                .ForMember(dest => dest.Tags, opts => opts.Ignore())
                .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(DeviceDetails.DeviceName), src.DeviceName))
                .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(DeviceDetails.ModelId), src.ModelId));

            _ = CreateMap<Twin, LoRaDeviceDetails>()
                .ForMember(dest => dest.DeviceID, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetails.DeviceName))))
                .ForMember(dest => dest.ModelId, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(DeviceDetails.ModelId))))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
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

            _ = CreateMap<LoRaDeviceDetails, Twin>()
               .AfterMap((src, dest) => dest.DeviceId = dest.DeviceId)
               .ForMember(dest => dest.Tags, opts => opts.Ignore())
               .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(LoRaDeviceDetails.DeviceName), src.DeviceName))
               .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(LoRaDeviceDetails.ModelId), src.ModelId))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.AppKey), src.AppKey))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.AppEUI), src.AppEUI))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.AppSKey), src.AppSKey))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.DevAddr), src.DevAddr))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.Downlink), src.Downlink))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.RX1DROffset), src.RX1DROffset))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.RX2DataRate), src.RX2DataRate))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.RXDelay), src.RXDelay))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.ABPRelaxMode), src.ABPRelaxMode))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.FCntUpStart), src.FCntUpStart))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.FCntDownStart), src.FCntDownStart))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.FCntResetCounter), src.FCntResetCounter))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.Supports32BitFCnt), src.Supports32BitFCnt))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.KeepAliveTimeout), src.KeepAliveTimeout))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.SensorDecoder), src.SensorDecoder))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.Deduplication), src.Deduplication))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.PreferredWindow), src.PreferredWindow))
               .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(LoRaDeviceDetails.ClassType), src.ClassType));
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
