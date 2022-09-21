// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using System.Collections.Generic;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.Azure.Devices.Shared;

    public class DeviceProfile : Profile
    {
        public DeviceProfile()
        {
            _ = CreateMap<Twin, Device>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Tags["deviceName"]))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.Tags["modelId"]))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)));

            _ = CreateMap<Twin, LorawanDevice>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Tags["deviceName"]))
                .ForMember(dest => dest.DeviceModelId, opts => opts.MapFrom(src => src.Tags["modelId"]))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.Tags, opts => opts.MapFrom(src => GetTags(src)))
                .ForMember(dest => dest.UseOTAA, opts => opts.MapFrom(src => !string.IsNullOrEmpty(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.AppEUI)))))
                .ForMember(dest => dest.AppKey, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.AppKey))))
                .ForMember(dest => dest.AppEUI, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.AppEUI))))
                .ForMember(dest => dest.AppSKey, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.AppSKey))))
                .ForMember(dest => dest.NwkSKey, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.NwkSKey))))
                .ForMember(dest => dest.DevAddr, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.DevAddr))))
                .ForMember(dest => dest.AlreadyLoggedInOnce, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveReportedPropertyValue(src, "DevAddr") != null))
                .ForMember(dest => dest.DataRate, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.DataRate))))
                .ForMember(dest => dest.TxPower, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.TxPower))))
                .ForMember(dest => dest.NbRep, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.NbRep))))
                .ForMember(dest => dest.ReportedRX2DataRate, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.ReportedRX2DataRate))))
                .ForMember(dest => dest.ReportedRX1DROffset, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.ReportedRX1DROffset))))
                .ForMember(dest => dest.ReportedRXDelay, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.ReportedRXDelay))))
                .ForMember(dest => dest.GatewayID, opts => opts.MapFrom(src => Helpers.DeviceHelper.RetrieveReportedPropertyValue(src, nameof(LoRaDeviceDetails.GatewayID))));
            //.ForMember(dest => dest.Downlink, opts => opts.MapFrom(src => bool.TryParse(Helpers.DeviceHelper.RetrieveDesiredPropertyValue(src, nameof(LoRaDeviceDetails.Downlink)), out bool result)));
        }

        private static Dictionary<string, string> GetTags(Twin twin)
        {
            var customTags = new Dictionary<string, string>();

            if (twin.Tags != null)
            {
                var tagList  = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(twin.Tags.ToJson());

                foreach (var tag in tagList)
                {
                    if (tag.Key is not "modelId" and not "deviceName")
                    {
                        customTags.Add(tag.Key, tag.Value.ToString());
                    }
                }
            }

            return customTags;
        }
    }
}
