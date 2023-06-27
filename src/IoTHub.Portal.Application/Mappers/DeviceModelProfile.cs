// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Shared;
    using Models.v10;
    using Models.v10.LoRaWAN;

    public class DeviceModelProfile : Profile
    {
        public DeviceModelProfile()
        {
            _ = CreateMap<DeviceModelDto, DeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ModelId))
                .ReverseMap();

            _ = CreateMap<LoRaDeviceModelDto, DeviceModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ModelId))
                .ReverseMap();

            _ = CreateMap<DeviceModelDto, ExternalDeviceModelDto>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description));
        }
    }
}
