// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Domain.Entities;
    using Microsoft.Azure.Devices.Shared;
    using Models.v10.LoRaWAN;
    using Shared.Models.v10;

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
                .ForMember(dest => dest.ClientThumbprint, opts => opts.MapFrom(src => DeviceHelper.RetrieveClientThumbprintValue(src)))
                .ForMember(dest => dest.LoraRegion, opts => opts.MapFrom(src => src.Tags["loraRegion"]))
                .ForMember(dest => dest.DeviceType, opts => opts.MapFrom(src => src.Tags["deviceType"]));

            _ = CreateMap<PaginatedResult<Concentrator>, PaginatedResult<ConcentratorDto>>();
        }
    }
}
