// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Infrastructure.Azure.Helpers;
    using Microsoft.Azure.Devices.Shared;
    using Models.v10.LoRaWAN;
    using Shared.Models.v1._0;

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
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(ConcentratorDto.DeviceName))))
                .ForMember(dest => dest.Version, opts => opts.MapFrom(src => src.Version))
                .ForMember(dest => dest.IsConnected, opts => opts.MapFrom(src => src.ConnectionState == Microsoft.Azure.Devices.DeviceConnectionState.Connected))
                .ForMember(dest => dest.IsEnabled, opts => opts.MapFrom(src => src.Status == Microsoft.Azure.Devices.DeviceStatus.Enabled))
                .ForMember(dest => dest.ClientThumbprint, opts => opts.MapFrom(src => DeviceHelper.RetrieveClientThumbprintValue(src)))
                .ForMember(dest => dest.LoraRegion, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(Concentrator.LoraRegion))))
                .ForMember(dest => dest.LoraRegion, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(Concentrator.DeviceType))));

            _ = CreateMap<PaginatedResult<Concentrator>, PaginatedResult<ConcentratorDto>>();

            _ = CreateMap<ConcentratorDto, Twin>()
                .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(ConcentratorDto.DeviceName), src.DeviceName))
                .AfterMap((src, dest) => DeviceHelper.SetTagValue(dest, nameof(ConcentratorDto.LoraRegion), src.LoraRegion))
                .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(ConcentratorDto.ClientThumbprint), src.ClientThumbprint))
                .AfterMap((src, dest) => DeviceHelper.SetDesiredProperty(dest, nameof(ConcentratorDto.RouterConfig), src.RouterConfig));

            _ = CreateMap<Twin, ConcentratorDto>()
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(ConcentratorDto.DeviceName))))
                .ForMember(dest => dest.LoraRegion, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(ConcentratorDto.LoraRegion))))
                .ForMember(dest => dest.ClientThumbprint, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(ConcentratorDto.ClientThumbprint))))
                .ForMember(dest => dest.RouterConfig, opts => opts.MapFrom(src => DeviceHelper.RetrieveTagValue(src, nameof(ConcentratorDto.RouterConfig)))); ;
        }
    }
}
