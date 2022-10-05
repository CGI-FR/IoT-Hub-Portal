// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using Models.v10.LoRaWAN;
    using Microsoft.Azure.Devices.Shared;
    using AzureIoTHub.Portal.Server.Extensions;
    using AzureIoTHub.Portal.Server.Helpers;
    using Newtonsoft.Json;

    public class ConcentratorProfile : Profile
    {
        public ConcentratorProfile()
        {
            _ = CreateMap<Concentrator, Concentrator>();

            _ = CreateMap<ConcentratorDto, Concentrator>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.DeviceName));

            _ = CreateMap<Concentrator, ConcentratorDto>()
                .ForMember(dest => dest.DeviceId, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => src.Name));

            _ = CreateMap<Twin, Concentrator>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Tags["deviceName"]))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.ClientThumbprint, opts => opts.MapFrom(src => RetrieveClientThumbprintValue(src)))
                .ForMember(dest => dest.LoraRegion, opts => opts.MapFrom(src => src.Tags["loraRegion"]))
                .ForMember(dest => dest.DeviceType, opts => opts.MapFrom(src => src.Tags["deviceType"]));
        }

        private static string RetrieveClientThumbprintValue(Twin twin)
        {
            var serializedClientThumbprint = DeviceHelper.RetrieveDesiredPropertyValue(twin, nameof(ConcentratorDto.ClientThumbprint).ToCamelCase());

            if (serializedClientThumbprint == null)
            {
                // clientThumbprint does not exist in the device twin
                return null;
            }

            try
            {
                var clientThumbprintArray=JsonConvert.DeserializeObject<string[]>(serializedClientThumbprint);

                if (clientThumbprintArray.Length == 0)
                {
                    // clientThumbprint array is empty in the device twin
                    return null;
                }

                return clientThumbprintArray[0];
            }
            catch (JsonReaderException)
            {
                // clientThumbprint is not an array in the device twin
                return null;
            }
        }
    }
}
